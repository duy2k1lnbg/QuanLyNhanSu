using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using NUnit.Framework;
using QLyNSu.FORM_CHAMCONG;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable, Apartment(ApartmentState.STA)]
    public class PayrollDataEditorRegressionTests
    {
        [SetUp] public void Setup() { UserSession.Clear(); Login(true); }
        [TearDown] public void Cleanup() { UserSession.Clear(); }
        public static void Login(bool edit)
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 80, USERNAME = "ADMIN", ISGROUP = 0 };
            UserSession.CurrentSessionId = "offline-test"; UserSession.CurrentJti = "offline-test"; UserSession.CurrentChannel = "DESKTOP";
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail> {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = true } },
                { "F_CC_BANGLUONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = edit, CAN_ADD = edit } }
            };
        }
        public static PayrollDataPage Page()
        {
            var section = PayrollPreparationDataService.Find("TB_NHANVIEN_THUE");
            var fields = section.Fields.Split(',').Select(n => new PayrollDataField { Name = n, Type = n.StartsWith("NGAY_") ? "DATE" : (n == "MANV" || PayrollPreparationDataService.IsFlag(n)) ? "NUMBER" : "VARCHAR2", Required = n == "MANV" || n == "NGAY_HIEU_LUC", Length = 200 }).ToList();
            var rows = new DataTable(); rows.Columns.Add("ID", typeof(decimal));
            foreach (var f in fields) rows.Columns.Add(f.Name, f.Type == "NUMBER" ? typeof(decimal) : f.Type == "DATE" ? typeof(DateTime) : typeof(string));
            for (int i = 30; i <= 31; i++) {
                var row = rows.NewRow(); row["ID"] = i; row["MANV"] = i;
                row["LOAI_DINH_DANH"] = "HO_CHIEU"; row["IS_CU_TRU"] = 1m;
                row["CAN_CU_CU_TRU"] = "CHUA_XAC_MINH"; row["CO_UY_QUYEN_QUYET_TOAN"] = 0m;
                row["TRANG_THAI_HO_SO"] = "THIEU_DINH_DANH"; row["NGAY_HIEU_LUC"] = new DateTime(2026, 1, 1); row["TRANG_THAI"] = "ACTIVE";
                rows.Rows.Add(row);
            }
            return new PayrollDataPage { Section = section, Fields = fields, Rows = rows };
        }
        public static Dictionary<string, DataTable> Lookups(PayrollDataPage page)
        {
            var lookup = new DataTable(); lookup.Columns.Add("ID", typeof(decimal)); lookup.Columns.Add("LABEL", typeof(string));
            lookup.Rows.Add(30m, "NV0030 - Nhân viên mẫu 30"); lookup.Rows.Add(31m, "NV0031 - Nhân viên mẫu 31");
            return page.Fields.ToDictionary(f => f.Name, f => f.Name == "MANV" ? lookup : null);
        }
        public static object Field(object form, string name) => form.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
        public static object Call(object form, string name, params object[] args) => form.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, args);
        public static Form Create(string table) => (Form)Activator.CreateInstance(typeof(FrmPayrollDataEditor), new object[] { 202610, table, null, null });
        public static void Populate(Form form) { var page = Page(); Call(form, "ApplyPage", page, Lookups(page)); }
        public static void HiddenHandle(Form form)
        {
            var handle = form.Handle;
            typeof(Control).GetMethod("SetState", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(form, new object[] { 2, true });
            form.CreateControl(); CreateChildren(form);
        }
        private static void CreateChildren(Control control) { foreach (Control child in control.Controls) { child.CreateControl(); CreateChildren(child); } }

        [Test]
        public void VerifiedTaxProfile_MissingIdentity_ExplainsAndTargetsPassport()
        {
            var page = Page(); var row = page.Rows.Rows[0];
            var values = page.Fields.ToDictionary(f => f.Name, f => row[f.Name]); values["TRANG_THAI_HO_SO"] = "DA_XAC_MINH";
            var error = Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(page.Section, values));
            Assert.That(error.ParamName, Is.EqualTo("SO_HO_CHIEU"));
            using (var form = Create(page.Section.Table)) {
                HiddenHandle(form); Populate(form);
                ((Task)Call(form, "Guard", new Func<Task>(() => Task.FromException(error)), "lưu dữ liệu")).GetAwaiter().GetResult();
                var feedback = (Control)Field(form, "_feedback");
                Assert.That(feedback.Text, Does.Contain("Mã số thuế hoặc Số hộ chiếu"));
                Assert.That(feedback.Text, Does.Not.Contain("REF-"));
                Assert.That(feedback.Text, Does.Not.Contain("Parameter name"));
            }
        }
        [Test]
        public void HistoricalProfileProtectionExplainsHowToProceed()
        {
            const string explanation = "Bản này đã dùng để tính lương. Chỉ được kết thúc hiệu lực sau kỳ đã dùng, rồi thêm phiên bản mới.";
            using (var form = Create("TB_NHANVIEN_THUE")) {
                HiddenHandle(form); Populate(form);
                ((Task)Call(form,"Guard",new Func<Task>(() => Task.FromException(new BusinessException("PAYROLL_PREPARATION_BLOCKED",explanation))),"lưu dữ liệu")).GetAwaiter().GetResult();
                Assert.That(((Control)Field(form,"_feedback")).Text, Is.EqualTo(explanation));
            }
        }
        [Test]
        public void DropdownsPreserveDatabaseCodes_AndSelectionLoadsCurrentRow()
        {
            using (var form = Create("TB_NHANVIEN_THUE")) {
                HiddenHandle(form); Populate(form);
                var values = (Dictionary<string, object>)Call(form, "ReadValues");
                Assert.That(values["LOAI_DINH_DANH"], Is.EqualTo("HO_CHIEU"));
                Assert.That(values["IS_CU_TRU"], Is.EqualTo(1m));
                Assert.That(values["MANV"], Is.EqualTo(30m));
                Assert.That(((Control)Field(form, "_save")).Enabled, Is.True);
                Assert.That(((Control)Field(form, "_sections")).Text, Does.Contain("Hồ sơ thuế"));
                var view = Field(form, "_view"); view.GetType().GetProperty("FocusedRowHandle").SetValue(view, 1);
                values = (Dictionary<string, object>)Call(form, "ReadValues");
                Assert.That(values["MANV"], Is.EqualTo(31m));
            }
        }
        [Test]
        public void ReloadKeepsReasonEditorAlive_AndReadonlyRightsStayReadonly()
        {
            Login(false);
            using (var form = Create("TB_NHANVIEN_THUE")) {
                HiddenHandle(form); Populate(form); Populate(form);
                var reason = (Control)Field(form, "_reason");
                Assert.That(reason.IsDisposed, Is.False);
                Assert.That(reason.Parent, Is.Not.Null);
                Assert.That(((Control)Field(form, "_save")).Enabled, Is.False);
                var editors = (System.Collections.IDictionary)Field(form, "_editors");
                foreach (var value in editors.Values) Assert.That(value.GetType().GetProperty("ReadOnly").GetValue(value), Is.True);
            }
        }
        [TestCase("Tiếng Anh")][TestCase("Tiếng Nhật")][TestCase("Tiếng Trung")][TestCase("Tiếng Hàn")]
        public void TaxEditorFeedbackAndLabelsAreTranslated(string language)
        {
            var setting = typeof(QLyNSu.Functions.TranslationManager).GetField("_currentLanguage", BindingFlags.Static | BindingFlags.NonPublic);
            var original = setting.GetValue(null);
            try {
                setting.SetValue(null, language);
                foreach (var text in new[] { "Hồ sơ thuế", "Loại định danh", "Căn cứ cư trú", "Ủy quyền quyết toán", "Trạng thái hồ sơ", "Dữ liệu chưa lưu", "Nhập căn cứ/lý do lưu dữ liệu.", "Hồ sơ đã xác minh phải có Mã số thuế hoặc Số hộ chiếu. Hãy bổ sung định danh trước khi lưu trạng thái Đã xác minh." })
                    Assert.That(QLyNSu.Functions.TranslationManager.Translate(text), Is.Not.EqualTo(text), text);
                using (var form = Create("TB_NHANVIEN_THUE")) {
                    form.Size = new Size(900,680); HiddenHandle(form); Populate(form); Call(form, "FitFields");
                    foreach (Control editor in ((System.Collections.IDictionary)Field(form,"_editors")).Values)
                        Assert.That(editor.Width, Is.GreaterThan(150));
                }
            } finally { setting.SetValue(null,original); }
        }
        [TestCase(900,680)]
        [TestCase(1180,800)]
        public void ResponsiveForm_HasSeparateListAndFields_AndUsableEditors(int width,int height)
        {
            using (var form = Create("TB_NHANVIEN_THUE")) {
                form.Size = new Size(width, height); HiddenHandle(form); Populate(form); form.PerformLayout(); Call(form,"FitFields");
                var split = (SplitContainer)Field(form,"_split");
                Assert.That(split.Orientation, Is.EqualTo(Orientation.Vertical));
                var editors = (System.Collections.IDictionary)Field(form,"_editors");
                foreach (Control editor in editors.Values) {
                    Assert.That(editor.Width, Is.GreaterThan(150), editor.Name);
                    Assert.That(editor.Font.SizeInPoints, Is.EqualTo(9.5f).Within(.1));
                }
                var fields = (TableLayoutPanel)Field(form,"_fields");
                Assert.That(fields.ColumnCount, Is.EqualTo(2));
                var grid = Field(form,"_view");
                Assert.That(grid.GetType().GetProperties().First(p => p.Name == "OptionsView" && p.GetIndexParameters().Length == 0).GetValue(grid).GetType().GetProperty("ShowAutoFilterRow").GetValue(grid.GetType().GetProperties().First(p => p.Name == "OptionsView" && p.GetIndexParameters().Length == 0).GetValue(grid)), Is.False);
            }
        }
    }
}
