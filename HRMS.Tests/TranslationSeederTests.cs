using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Bu.CLASS_SYSTEM;
using DA;

namespace HRMS.Tests
{
    [TestFixture]
    public class TranslationSeederTests
    {
        [SetUp]
        public void Setup()
        {
            var sysUser = new SYS_USER();
            sysUser.EnsureSeeded();
        }

        [Test]
        public void EnsureSeeded_SeedsNewUiLabels_InAllFourLanguages()
        {
            using (var db = new MyEntities())
            {
                var coreLabels = new[]
                {
                    "Phân hệ",
                    "Phê Duyệt Yêu Cầu (Online)",
                    "Thông Báo Hệ Thống",
                    "Cấp Tài Khoản Hàng Loạt",
                    "Cấu Hình Kết Nối CSDL",
                    "Chọn tất cả",
                    "Bỏ tất cả",
                    "Sửa quyền",
                    "Làm mới"
                };

                var requiredLanguages = new[] { "EN", "JA", "ZH", "KO" };

                foreach (var label in coreLabels)
                {
                    var translations = db.TB_TRANSLATIONS
                        .Where(t => t.TABLE_NAME == "UI_LABEL" && t.COLUMN_NAME == label)
                        .ToList();

                    Assert.IsNotEmpty(translations, $"Expected translations for '{label}' to be present in TB_TRANSLATIONS");

                    foreach (var lang in requiredLanguages)
                    {
                        var match = translations.FirstOrDefault(t => t.LANGUAGE_CODE == lang);
                        Assert.IsNotNull(match, $"Expected '{lang}' translation for '{label}'");
                        Assert.IsNotEmpty(match.VALUE, $"Expected non-empty value for '{label}' in '{lang}'");
                    }
                }
            }
        }

        [Test]
        public void EnsureSeeded_AllFiftyEightFunctions_HaveValidDescriptions()
        {
            using (var db = new MyEntities())
            {
                var functions = db.TB_SYS_FUNCTION.ToList();
                Assert.AreEqual(58, functions.Count, "System should have exactly 58 seeded functions");

                foreach (var f in functions)
                {
                    Assert.IsNotNull(f.FUNCTION_CODE, "Function code cannot be null");
                    Assert.IsNotEmpty(f.FUNCTION_CODE, "Function code cannot be empty");
                    Assert.IsNotNull(f.DESCRIPTION, $"Description for {f.FUNCTION_CODE} cannot be null");
                    Assert.IsNotEmpty(f.DESCRIPTION, $"Description for {f.FUNCTION_CODE} cannot be empty");
                    Assert.IsNotNull(f.PARENT, $"Parent for {f.FUNCTION_CODE} cannot be null");
                }
            }
        }

        [Test]
        public void TranslationManager_InitializesWithoutTypeInitializationException()
        {
            var desktopPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "HRMS.Desktop", "bin", "Debug", "QLyNSu.exe"));
            Assert.IsTrue(System.IO.File.Exists(desktopPath), $"Desktop exe should exist at: {desktopPath}");

            var asm = System.Reflection.Assembly.LoadFrom(desktopPath);
            var type = asm.GetType("QLyNSu.Functions.TranslationManager");
            Assert.IsNotNull(type, "TranslationManager type should exist in QLyNSu.exe");

            // Invoking LoadLanguage must NOT throw TypeInitializationException or ArgumentException
            var loadLangMethod = type.GetMethod("LoadLanguage");
            Assert.DoesNotThrow(() => loadLangMethod.Invoke(null, null));

            // Test translating keys in English
            var currentLangProp = type.GetProperty("CurrentLanguage");
            currentLangProp.SetValue(null, "Tiếng Anh", null);

            var translateMethod = type.GetMethod("Translate", new[] { typeof(string) });
            var translated = (string)translateMethod.Invoke(null, new object[] { "Phân hệ" });
            Assert.AreEqual("Subsystem", translated);

            var translated2 = (string)translateMethod.Invoke(null, new object[] { "Cấp Tài Khoản Hàng Loạt" });
            Assert.AreEqual("Bulk Account Provisioning", translated2);

            // Test Japanese
            currentLangProp.SetValue(null, "Tiếng Nhật", null);
            var translatedJa = (string)translateMethod.Invoke(null, new object[] { "Phân hệ" });
            Assert.AreEqual("サブシステム", translatedJa);

            // Reset back
            currentLangProp.SetValue(null, "Tiếng Việt", null);
        }
    }
}
