using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_SYSTEM;
using DA;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class AttendancePeriodRegressionTests
    {
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static Type DetailType => typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG.FrmBangCong_ChiTiet", true);

        [TestCase(202609, 2026, 9)]
        [TestCase(202603, 2026, 3)]
        [TestCase(202610, 2026, 10)]
        [TestCase(202512, 2025, 12)]
        [Apartment(ApartmentState.STA)]
        public void CallerPeriodWinsOverCurrentMonthAndConflictingFields(int period, int year, int month)
        {
            var type = DetailType;
            using (var form = (Form)Activator.CreateInstance(type))
            {
                type.GetField("_MAKYCONG").SetValue(form, period);
                type.GetField("_nam").SetValue(form, 2026);
                type.GetField("_thang").SetValue(form, 10);
                type.GetMethod("InitializePeriodSelection", Private).Invoke(form, null);
                Assert.That(type.GetField("_MAKYCONG").GetValue(form), Is.EqualTo(period));
                Assert.That(type.GetField("_nam").GetValue(form), Is.EqualTo(year));
                Assert.That(type.GetField("_thang").GetValue(form), Is.EqualTo(month));
                Assert.That(((Control)type.GetField("cboNam", Private).GetValue(form)).Text, Is.EqualTo(year.ToString()));
                Assert.That(((Control)type.GetField("cboThang", Private).GetValue(form)).Text, Is.EqualTo(month.ToString()));
            }
        }

        [TestCase(202600)]
        [TestCase(202613)]
        [Apartment(ApartmentState.STA)]
        public void InvalidCallerKeyIsRejectedInsteadOfOpeningCurrentMonth(int period)
        {
            var type = DetailType;
            using (var form = (Form)Activator.CreateInstance(type))
            {
                type.GetField("_MAKYCONG").SetValue(form, period);
                var error = Assert.Throws<TargetInvocationException>(() => type.GetMethod("InitializePeriodSelection", Private).Invoke(form, null));
                Assert.That(error.InnerException, Is.TypeOf<ArgumentException>());
            }
        }

        [Test, Apartment(ApartmentState.STA)]
        public void LegacyYearMonthCallerStillOpensItsRequestedPeriod()
        {
            var type = DetailType;
            using (var form = (Form)Activator.CreateInstance(type))
            {
                type.GetField("_nam").SetValue(form, 2024);
                type.GetField("_thang").SetValue(form, 2);
                type.GetMethod("InitializePeriodSelection", Private).Invoke(form, null);
                Assert.That(type.GetField("_MAKYCONG").GetValue(form), Is.EqualTo(202402));
            }
        }

        [TestCase(202610, 2026, 9)]
        [TestCase(202609, 2025, 9)]
        public void CreatingMismatchedPeriodKeyFailsBeforeAccessingDatabase(int period, int year, int month)
        {
            bool contextRequested = false;
            var service = new KYCONG(() => { contextRequested = true; throw new Exception("Database must not be reached"); });
            var error = Assert.Throws<BusinessException>(() => service.Add(new TB_KYCONG { MAKYCONG = period, NAM = year, THANG = month }));
            Assert.That(error.Message, Does.Contain("Mã kỳ công"));
            Assert.That(contextRequested, Is.False);
        }

        [TestCase(0, "Chưa công bố")]
        [TestCase(1, "Đã công bố")]
        [Apartment(ApartmentState.STA)]
        public void ExistingPeriodIsDisplayedByPublicationStateInsteadOfMissingPeriod(int state, string expected)
        {
            var type = typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG.FrmBangCong", true);
            using (var form = (Form)Activator.CreateInstance(type))
            {
                form.Load -= (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), form, type.GetMethod("FrmBangCong_Load", Private));
                var grid = (Control)type.GetField("gcDanhSach", Private).GetValue(form);
                grid.GetType().GetProperty("DataSource").SetValue(grid, new[] { new TB_KYCONG { MAKYCONG = 202610, NAM = 2026, THANG = 10, TRANGTHAI = state } });
                form.ShowInTaskbar = false; form.Opacity = 0; form.StartPosition = FormStartPosition.Manual;
                form.Location = new System.Drawing.Point(-30000, -30000);
                form.Show(); Application.DoEvents();
                var view = type.GetField("gvDanhSach", Private).GetValue(form);
                var formatter = view.GetType().GetMethod("GetRowCellDisplayText", new[] { typeof(int), typeof(string) });
                Assert.That(formatter.Invoke(view, new object[] { 0, "TRANGTHAI" }), Is.EqualTo(QLyNSu.Functions.TranslationManager.Translate(expected)));
            }
        }

        [TestCase(12L, 11L, 3L, 3L, true)]
        [TestCase(12L, 12L, 4L, 3L, true)]
        [TestCase(12L, 12L, 3L, 3L, false)]
        [TestCase(0L, 0L, 0L, 0L, false)]
        public void PublicationRequiresStableInputAndOutputRevisions(long currentInput, long loadedInput, long currentOutput, long loadedOutput, bool expected)
        {
            var guard = typeof(AttendancePublishingService).GetMethod("HasRevisionDrift", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(guard, Is.Not.Null);
            Assert.That(guard.Invoke(null, new object[] { currentInput, loadedInput, currentOutput, loadedOutput }), Is.EqualTo(expected));
        }
    }
}
