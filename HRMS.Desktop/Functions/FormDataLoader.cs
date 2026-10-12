using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.Functions
{
    /// <summary>Serializes reads per form and keeps only that form busy. UI binding remains on the UI thread.</summary>
    public static class FormDataLoader
    {
        private sealed class LoadState
        {
            public readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
            public readonly CancellationTokenSource Lifetime = new CancellationTokenSource();
            public int Pending;
            public bool WasEnabled;
            public bool WasWaiting;
        }
        private static readonly ConditionalWeakTable<Form, LoadState> States = new ConditionalWeakTable<Form, LoadState>();

        public static async Task<T> ReadAsync<T>(Form owner, Func<T> read)
        {
            if (owner == null || read == null) throw new ArgumentNullException();
            if (owner.InvokeRequired) throw new InvalidOperationException("Start form data loads on the UI thread.");
            if (owner.IsDisposed || owner.Disposing) throw new OperationCanceledException();
            var state = States.GetValue(owner, form => {
                var created = new LoadState();
                form.FormClosed += (s, e) => created.Lifetime.Cancel();
                form.Disposed += (s, e) => created.Lifetime.Cancel();
                return created;
            });
            var busy = KeepBusy(owner, state);
            bool entered = false;
            try
            {
                await state.Gate.WaitAsync(state.Lifetime.Token);
                entered = true;
                var result = await Task.Run(read, state.Lifetime.Token);
                if (owner.IsDisposed || owner.Disposing || state.Lifetime.IsCancellationRequested)
                    throw new OperationCanceledException();
                return result;
            }
            finally
            {
                if (entered) state.Gate.Release();
                busy.Dispose();
            }
        }
        private sealed class BusyScope : IDisposable
        {
            private readonly Form _owner;
            private readonly LoadState _state;
            private bool _disposed;
            public BusyScope(Form owner, LoadState state) { _owner = owner; _state = state; }
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                if (--_state.Pending == 0 && !_owner.IsDisposed && !_owner.Disposing)
                {
                    _owner.Enabled = _state.WasEnabled;
                    _owner.UseWaitCursor = _state.WasWaiting;
                }
            }
        }
        private static IDisposable KeepBusy(Form owner, LoadState state)
        {
            if (state.Pending++ == 0)
            {
                state.WasEnabled = owner.Enabled;
                state.WasWaiting = owner.UseWaitCursor;
                owner.Enabled = false;
                owner.UseWaitCursor = true;
            }
            return new BusyScope(owner, state);
        }
        internal static IDisposable KeepBusy(Form owner)
        {
            if (owner.IsDisposed || owner.Disposing) throw new OperationCanceledException();
            var state = States.GetValue(owner, form => {
                var created = new LoadState();
                form.FormClosed += (s, e) => created.Lifetime.Cancel();
                form.Disposed += (s, e) => created.Lifetime.Cancel();
                return created;
            });
            return KeepBusy(owner, state);
        }
        public static Task<TResult> ReadAsync<TInput, TResult>(Form owner, TInput snapshot, Func<TInput, TResult> read)
        {
            return ReadAsync(owner, () => read(snapshot));
        }
        public static Task RunAsync(Form owner, Action read)
        {
            return ReadAsync(owner, () => { read(); return true; });
        }
        public static void ShowLoadError(Form owner, Exception error)
        {
            if (owner.IsDisposed || owner.Disposing || error is OperationCanceledException) return;
            var text = Bu.CLASS_SYSTEM.ErrorHelper.ResolveUserFriendlyMessage(error, "tải dữ liệu");
            DevExpress.XtraEditors.XtraMessageBox.Show(owner, TranslationManager.Translate(text), TranslationManager.Translate("Thông báo"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
