using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QLyNSu.Functions
{
    /// <summary>Await a child editor without the application-wide modal loop of ShowDialog.</summary>
    public static class ChildFormWindow
    {
        public static Task<DialogResult> ShowAsync(Form owner, Form child)
        {
            if (owner == null || child == null) throw new ArgumentNullException();
            if (owner.IsDisposed || owner.Disposing) return Task.FromResult(DialogResult.Cancel);
            if (owner.InvokeRequired) throw new InvalidOperationException("Open child windows on the UI thread.");
            var completion = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            // Keep the main MDI workspace available. An editor's own parent remains stable until it closes.
            var busy = owner.IsMdiContainer ? null : FormDataLoader.KeepBusy(owner);
            FormClosedEventHandler ownerClosed = null;
            EventHandler ownerDisposed = null;
            FormClosedEventHandler childClosed = null;
            EventHandler childDisposed = null;
            bool finished = false;
            Action<DialogResult> finish = result => {
                if (finished) return;
                finished = true;
                owner.FormClosed -= ownerClosed;
                owner.Disposed -= ownerDisposed;
                child.FormClosed -= childClosed;
                child.Disposed -= childDisposed;
                busy?.Dispose();
                completion.TrySetResult(result);
            };
            Action closeWithOwner = () => { finish(DialogResult.Cancel); if (!child.IsDisposed) child.Dispose(); };
            ownerClosed = (sender, args) => closeWithOwner();
            ownerDisposed = (sender, args) => closeWithOwner();
            childClosed = (sender, args) => finish(owner.IsDisposed || owner.Disposing ? DialogResult.Cancel : child.DialogResult);
            childDisposed = (sender, args) => finish(DialogResult.Cancel);
            owner.FormClosed += ownerClosed;
            owner.Disposed += ownerDisposed;
            child.FormClosed += childClosed;
            child.Disposed += childDisposed;
            try { child.Show(owner); }
            catch { finish(DialogResult.Cancel); throw; }
            return completion.Task;
        }
    }
}
