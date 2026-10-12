using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.Services.AI_Services;
using Bu.Services.AI_Services.Core;
using NUnit.Framework;
using QLyNSu.Functions;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiChatPresentationTests
    {
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(500)]
        [TestCase(1000)]
        [TestCase(10000)]
        public void LongerAnswersRevealFasterWithBoundedDuration(int words)
        {
            var timeline = new AiTypingTimeline(string.Concat(Enumerable.Repeat("từ ", words)));
            var shorter = new AiTypingTimeline(string.Concat(Enumerable.Repeat("từ ", words / 2)));
            Assert.That(timeline.DurationMs, Is.LessThanOrEqualTo(6000));
            Assert.That(timeline.WordCount / timeline.DurationMs, Is.GreaterThan(shorter.WordCount / shorter.DurationMs));
        }

        [TestCase("")]
        [TestCase("  \n\t")]
        [TestCase("  Xin chào 👩‍💻!\r\nDòng cuối: 1.234.567 ₫.  ")]
        public void AnimationPreservesExactText(string text)
        {
            var timeline = new AiTypingTimeline(text);
            string previous = "", reconstructed = "";
            for (double elapsed = 0; elapsed <= timeline.DurationMs + 30; elapsed += 30)
            {
                string shown = text.Substring(0, timeline.VisibleLength(elapsed));
                Assert.That(shown.StartsWith(previous, StringComparison.Ordinal), Is.True);
                reconstructed += shown.Substring(previous.Length);
                previous = shown;
            }
            Assert.That(reconstructed, Is.EqualTo(text));
        }

        [Test]
        public void DisplayChunksKeepAllUnicodeAndFinalParagraph()
        {
            string text = new string('a', 1599) + "👩‍💻" + string.Concat(Enumerable.Repeat(" a\u0301\r\n", 9000)) + "KẾT THÚC";
            var chunks = AiTypingTimeline.SplitForDisplay(text);
            Assert.That(string.Concat(chunks), Is.EqualTo(text));
            Assert.That(chunks.All(c => c.Length > 0 && c.Length <= 1600), Is.True);
            Assert.That(chunks.All(c => !char.IsHighSurrogate(c[c.Length - 1])), Is.True);
            Assert.That(chunks.All(c => !char.IsLowSurrogate(c[0]) && c[0] != '\u0301'), Is.True);
        }

        [Test]
        public async Task VisibleHistoryRetainsMoreThanTenTurnsUntilExplicitReset()
        {
            var manager = new ChatboxManager(remoteChatHandler: (q, c, o, v, cl, ct) => Task.FromResult(
                new AiChatExecutionResult { Status = "answered", Answer = "Trả lời " + q }));
            for (int i = 0; i < 15; i++) await manager.ProcessQueryAsync("Câu " + i);
            Assert.That(manager.GetMessages().Count, Is.EqualTo(30));
            Assert.That(manager.GetMessages()[0].Content, Is.EqualTo("Câu 0"));
            manager.Reset();
            Assert.That(manager.GetMessages(), Is.Empty);
        }

        [Test, Apartment(ApartmentState.STA)]
        public void LongDesktopAnswerKeepsEarlierAndFinalMessagesAndScrollsToTailAfterResize()
        {
            // Reflection keeps this test independent of the DevExpress public API.
            var type = typeof(AiTypingTimeline).Assembly.GetType("QLyNSu.FORM_SYSTEM.FrmAI_Chat", true);
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            using (var form = (Form)Activator.CreateInstance(type))
            {
                var flow = (FlowLayoutPanel)type.GetField("flpChat", flags).GetValue(form);
                // Layout/scrollbars are only realized for a visible native window.
                // Render transparently off screen so the regression test never takes over the desktop.
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new System.Drawing.Point(-30000, -30000);
                form.Show();
                Application.DoEvents();
                type.GetMethod("ClearChatControls", flags).Invoke(form, null);
                var add = type.GetMethod("AddMessageBubble", flags);
                string longAnswer = string.Concat(Enumerable.Repeat("Thông tin nhân viên, lương và phòng ban.\n", 1200)) + "KẾT THÚC";
                add.Invoke(form, new object[] { "You", "TIN NHẮN ĐẦU" });
                object[] placeholder = { null };
                var liveLabel = type.GetMethod("AddStreamingAiBubble", flags).Invoke(form, placeholder);
                var update = type.GetMethod("UpdateStreamingAiBubble", flags);
                foreach (int length in new[] { 20, 1800, 9000, longAnswer.Length })
                {
                    update.Invoke(form, new[] { placeholder[0], liveLabel, longAnswer.Substring(0, length) });
                    Assert.That(flow.Controls[flow.Controls.Count - 1].Bottom, Is.LessThanOrEqualTo(flow.ClientSize.Height));
                }
                add.Invoke(form, new object[] { "You", "TIN NHẮN CUỐI" });
                int count = flow.Controls.Count;
                Assert.That(count, Is.GreaterThan(3));
                form.Width = 450;
                flow.PerformLayout();
                type.GetMethod("ScrollChatToBottom", flags).Invoke(form, null);
                Assert.That(flow.Controls.Count, Is.EqualTo(count));
                Assert.That(flow.Controls[0].Controls[0].Text, Does.Contain("TIN NHẮN ĐẦU"));
                Assert.That(flow.Controls[count - 2].Controls[0].Text, Does.Contain("KẾT THÚC"));
                Assert.That(flow.Controls[count - 1].Controls[0].Text, Does.Contain("TIN NHẮN CUỐI"));
                Assert.That(flow.Controls.Cast<Control>().All(c => c.Height > 20 && c.Height < 10000), Is.True);
                Assert.That(-flow.AutoScrollPosition.Y, Is.GreaterThan(0));
                TestContext.WriteLine("Flow: client=" + flow.ClientSize + "; display=" + flow.DisplayRectangle + "; scroll=" + flow.AutoScrollPosition
                    + "; maximum=" + flow.VerticalScroll.Maximum + "; large=" + flow.VerticalScroll.LargeChange + "; value=" + flow.VerticalScroll.Value
                    + "; padding=" + flow.Padding + "; last=" + flow.Controls[count - 1].Bounds + "; horizontal=" + flow.HorizontalScroll.Visible);
                Assert.That(flow.Controls[count - 1].Bottom, Is.LessThanOrEqualTo(flow.ClientSize.Height));
                var expectedChunks = AiTypingTimeline.SplitForDisplay(longAnswer);
                for (int i = 0; i < expectedChunks.Count; i++)
                {
                    string html = flow.Controls[i + 1].Controls[0].Text;
                    if (i == 0) html = html.Substring(html.IndexOf("<br/>", StringComparison.Ordinal) + 5);
                    string displayed = System.Net.WebUtility.HtmlDecode(html.Replace("<br/>", "\n"));
                    Assert.That(displayed, Is.EqualTo(expectedChunks[i]));
                }
                // Clearing during animation disposes every continuation; old answers cannot reappear.
                var first = (Panel)flow.Controls[1];
                var label = first.Controls[0];
                type.GetMethod("ResetChatUI", flags).Invoke(form, null);
                Assert.That(first.IsDisposed && label.IsDisposed, Is.True);
                Assert.That(flow.Controls.Count, Is.EqualTo(1));
                var handler = (Action)Delegate.CreateDelegate(typeof(Action), form, type.GetMethod("OnSessionCleared", flags));
                Bu.CLASS_SYSTEM.UserSession.SessionCleared -= handler;
            }
        }
    }
}
