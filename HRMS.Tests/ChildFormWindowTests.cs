using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using QLyNSu.Functions;

namespace HRMS.Tests
{
    [TestFixture, Apartment(ApartmentState.STA), NonParallelizable]
    public class ChildFormWindowTests
    {
        private SynchronizationContext _previous;
        [SetUp] public void Setup() { _previous = SynchronizationContext.Current; SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext()); }
        [TearDown] public void Cleanup() { SynchronizationContext.SetSynchronizationContext(_previous); }
        private static Form Window(bool mdi = false)
        {
            var f = new Form { ShowInTaskbar = false, Opacity = 0, StartPosition = FormStartPosition.Manual, Location = new Point(-30000, -30000), IsMdiContainer = mdi };
            f.Show(); return f;
        }
        private static void PumpUntil(Func<bool> done)
        {
            var watch = Stopwatch.StartNew();
            while (!done() && watch.ElapsedMilliseconds < 5000) { Application.DoEvents(); Thread.Sleep(1); }
            Assert.That(done(), Is.True);
        }
        [Test] public void ChildKeepsItsParentStableWithoutDisablingOtherWindow()
        {
            using (var parent = Window()) using (var other = Window()) using (var child = Window())
            {
                child.Hide(); var result = ChildFormWindow.ShowAsync(parent, child);
                Assert.That(result.IsCompleted, Is.False); Assert.That(parent.Enabled, Is.False); Assert.That(other.Enabled, Is.True);
                SynchronizationContext.Current.Post(_ => other.Text = "still usable", null);
                PumpUntil(() => other.Text == "still usable");
                child.DialogResult = DialogResult.OK; child.Close(); PumpUntil(() => result.IsCompleted);
                Assert.That(result.GetAwaiter().GetResult(), Is.EqualTo(DialogResult.OK)); Assert.That(parent.Enabled, Is.True);
            }
        }
        [Test] public void MainMdiWorkspaceRemainsEnabledWhileChildIsOpen()
        {
            using (var main = Window(true)) using (var child = Window())
            {
                child.Hide(); var result = ChildFormWindow.ShowAsync(main, child);
                Assert.That(main.Enabled, Is.True); child.Close(); PumpUntil(() => result.IsCompleted);
            }
        }
        [Test] public void ChildCompletionDoesNotUnlockParentWhileItsReadIsPending()
        {
            using (var parent = Window()) using (var child = Window()) using (var release = new ManualResetEventSlim())
            {
                child.Hide(); var dialog = ChildFormWindow.ShowAsync(parent, child);
                var read = FormDataLoader.ReadAsync(parent, () => { release.Wait(4000); return 42; });
                try { child.Close(); PumpUntil(() => dialog.IsCompleted); Assert.That(parent.Enabled, Is.False); }
                finally { release.Set(); }
                PumpUntil(() => read.IsCompleted); Assert.That(read.GetAwaiter().GetResult(), Is.EqualTo(42)); Assert.That(parent.Enabled, Is.True);
            }
        }
        [Test] public void DisposingParentClosesChildAndReturnsCancel()
        {
            using (var parent = Window()) using (var child = Window())
            {
                child.Hide(); var result = ChildFormWindow.ShowAsync(parent, child); parent.Dispose();
                PumpUntil(() => result.IsCompleted);
                Assert.That(result.GetAwaiter().GetResult(), Is.EqualTo(DialogResult.Cancel)); Assert.That(child.IsDisposed, Is.True);
            }
        }
    }
}
