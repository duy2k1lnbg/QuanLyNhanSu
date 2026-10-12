using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using QLyNSu.Functions;

namespace HRMS.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class FormDataLoaderTests
    {
        private SynchronizationContext _previous;
        [SetUp] public void Setup() { _previous = SynchronizationContext.Current; SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext()); }
        [TearDown] public void Cleanup() { SynchronizationContext.SetSynchronizationContext(_previous); }
        private static Form Owner() { var f = new Form(); var handle = f.Handle; return f; }
        private static void PumpUntil(Func<bool> done)
        {
            var watch = Stopwatch.StartNew();
            while (!done() && watch.ElapsedMilliseconds < 5000) { Application.DoEvents(); Thread.Sleep(1); }
            Assert.That(done(), Is.True, "The UI continuation did not complete.");
        }
        private static async Task<int> Bind(Form owner, Func<int> read)
        {
            var value = await FormDataLoader.ReadAsync(owner, read);
            owner.Text = value.ToString();
            return Thread.CurrentThread.ManagedThreadId;
        }
        [Test] public void SlowReadLeavesOtherWindowInteractiveAndBindsOnUiThread()
        {
            using (var owner = Owner()) using (var other = Owner()) using (var release = new ManualResetEventSlim())
            {
                int uiThread = Thread.CurrentThread.ManagedThreadId, worker = 0;
                var task = Bind(owner, () => { worker = Thread.CurrentThread.ManagedThreadId; release.Wait(4000); return 42; });
                try {
                    Assert.That(owner.Enabled, Is.False); Assert.That(other.Enabled, Is.True);
                    SynchronizationContext.Current.Post(_ => other.Text = "interactive", null);
                    PumpUntil(() => other.Text == "interactive");
                    Assert.That(task.IsCompleted, Is.False);
                } finally { release.Set(); }
                PumpUntil(() => task.IsCompleted);
                Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(uiThread));
                Assert.That(worker, Is.Not.EqualTo(uiThread)); Assert.That(owner.Text, Is.EqualTo("42"));
                Assert.That(owner.Enabled, Is.True); Assert.That(owner.UseWaitCursor, Is.False);
            }
        }
        [Test] public void ReadsForSameFormAreSerializedAndBusyStateLastsUntilBothFinish()
        {
            using (var owner = Owner()) using (var release = new ManualResetEventSlim())
            {
                int secondStarted = 0;
                var first = FormDataLoader.ReadAsync(owner, () => { release.Wait(4000); return 1; });
                var second = FormDataLoader.ReadAsync(owner, () => { Interlocked.Exchange(ref secondStarted, 1); return 2; });
                try { Application.DoEvents(); Assert.That(secondStarted, Is.Zero); Assert.That(owner.Enabled, Is.False); }
                finally { release.Set(); }
                PumpUntil(() => first.IsCompleted && second.IsCompleted);
                Assert.That(first.GetAwaiter().GetResult(), Is.EqualTo(1)); Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(2));
                Assert.That(owner.Enabled, Is.True);
            }
        }
        [Test] public void DifferentFormsDoNotShareLoadLock()
        {
            using (var firstOwner = Owner()) using (var secondOwner = Owner()) using (var release = new ManualResetEventSlim())
            {
                var first = FormDataLoader.ReadAsync(firstOwner, () => { release.Wait(4000); return 1; });
                var second = FormDataLoader.ReadAsync(secondOwner, () => 2);
                try { PumpUntil(() => second.IsCompleted); Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(2)); Assert.That(first.IsCompleted, Is.False); }
                finally { release.Set(); }
                PumpUntil(() => first.IsCompleted); first.GetAwaiter().GetResult();
            }
        }
        [Test] public void DisposalCancelsQueuedReadAndIgnoresLateResult()
        {
            using (var owner = Owner()) using (var release = new ManualResetEventSlim())
            {
                int queuedReadRan = 0;
                var first = FormDataLoader.ReadAsync(owner, () => { release.Wait(4000); return 1; });
                var second = FormDataLoader.ReadAsync(owner, () => { Interlocked.Exchange(ref queuedReadRan, 1); return 2; });
                owner.Dispose(); release.Set();
                PumpUntil(() => first.IsCompleted && second.IsCompleted);
                Assert.Catch<OperationCanceledException>(() => first.GetAwaiter().GetResult());
                Assert.Catch<OperationCanceledException>(() => second.GetAwaiter().GetResult());
                Assert.That(queuedReadRan, Is.Zero);
            }
        }
        [Test] public void FailureRestoresOwnerState()
        {
            using (var owner = Owner())
            {
                var task = FormDataLoader.ReadAsync<int>(owner, () => { throw new InvalidOperationException("simulated read error"); });
                PumpUntil(() => task.IsCompleted);
                Assert.Throws<InvalidOperationException>(() => task.GetAwaiter().GetResult());
                Assert.That(owner.Enabled, Is.True); Assert.That(owner.UseWaitCursor, Is.False);
            }
        }
        [Test] public void ExistingDisabledAndWaitingStateIsPreserved()
        {
            using (var owner = Owner())
            {
                owner.Enabled = false; owner.UseWaitCursor = true;
                var task = FormDataLoader.ReadAsync(owner, () => 1);
                PumpUntil(() => task.IsCompleted); task.GetAwaiter().GetResult();
                Assert.That(owner.Enabled, Is.False); Assert.That(owner.UseWaitCursor, Is.True);
            }
        }
    }
}
