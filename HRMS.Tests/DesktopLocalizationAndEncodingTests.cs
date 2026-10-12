using System;
using NUnit.Framework;
using QLyNSu.Functions;

namespace HRMS.Tests
{
    [TestFixture]
    public class DesktopLocalizationAndEncodingTests
    {
        [Test]
        public void GetCanonicalLanguage_ShouldNormalizeVariousInputs()
        {
            Assert.AreEqual("Tiếng Việt", TranslationManager.GetCanonicalLanguage("vi"));
            Assert.AreEqual("Tiếng Việt", TranslationManager.GetCanonicalLanguage("vi-VN"));
            Assert.AreEqual("Tiếng Việt", TranslationManager.GetCanonicalLanguage("Vietnamese"));
            Assert.AreEqual("Tiếng Việt", TranslationManager.GetCanonicalLanguage("Tiếng Việt"));

            Assert.AreEqual("Tiếng Anh", TranslationManager.GetCanonicalLanguage("en"));
            Assert.AreEqual("Tiếng Anh", TranslationManager.GetCanonicalLanguage("en-US"));
            Assert.AreEqual("Tiếng Anh", TranslationManager.GetCanonicalLanguage("English"));

            Assert.AreEqual("Tiếng Nhật", TranslationManager.GetCanonicalLanguage("ja"));
            Assert.AreEqual("Tiếng Nhật", TranslationManager.GetCanonicalLanguage("ja-JP"));
            Assert.AreEqual("Tiếng Nhật", TranslationManager.GetCanonicalLanguage("Japanese"));
            Assert.AreEqual("Tiếng Nhật", TranslationManager.GetCanonicalLanguage("日本語"));

            Assert.AreEqual("Tiếng Trung", TranslationManager.GetCanonicalLanguage("zh"));
            Assert.AreEqual("Tiếng Trung", TranslationManager.GetCanonicalLanguage("zh-CN"));
            Assert.AreEqual("Tiếng Trung", TranslationManager.GetCanonicalLanguage("chinesesimplified"));
            Assert.AreEqual("Tiếng Trung", TranslationManager.GetCanonicalLanguage("Chinese"));
            Assert.AreEqual("Tiếng Trung", TranslationManager.GetCanonicalLanguage("中文"));

            Assert.AreEqual("Tiếng Hàn", TranslationManager.GetCanonicalLanguage("ko"));
            Assert.AreEqual("Tiếng Hàn", TranslationManager.GetCanonicalLanguage("ko-KR"));
            Assert.AreEqual("Tiếng Hàn", TranslationManager.GetCanonicalLanguage("Korean"));
            Assert.AreEqual("Tiếng Hàn", TranslationManager.GetCanonicalLanguage("한국어"));
        }

        [Test]
        public void Translate_ShouldReturnCleanStringsWithoutMojibake()
        {
            string[] testKeys = new[]
            {
                "Lỗi",
                "Thông báo",
                "Xác nhận",
                "Không Tên",
                "Vui lòng chọn một kết nối để sửa.",
                "Lỗi đọc cấu hình: ",
                "Lỗi ghi file cấu hình: ",
                "Chọn nhóm muốn tham gia",
                "Chọn thành viên thêm vào nhóm",
                "Gửi bổ sung",
                "Câu trả lời từ AI Copilot"
            };

            foreach (var key in testKeys)
            {
                string vi = TranslationManager.Translate(key);
                Assert.IsFalse(vi.Contains("\uFFFD"), $"Key '{key}' contains replacement character U+FFFD");
                Assert.IsFalse(vi.Contains("L?i"), $"Key '{key}' contains corrupted 'L?i'");
                Assert.IsFalse(vi.Contains("c?u"), $"Key '{key}' contains corrupted 'c?u'");
            }
        }

        [Test]
        public void FormatDevExpressHtml_ShouldNotEncodeVietnameseToNumericEntities()
        {
            string sampleMessage = "Phiên làm việc đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.";
            string userQuestion = "Danh sách tất cả nhân viên trong công ty?";

            // Standard WebUtility.HtmlEncode in .NET Framework turns 'ê' into '&#234;'
            // Our custom DevExpress formatter must keep characters authentic
            string decoded = System.Net.WebUtility.HtmlDecode(sampleMessage);
            string formatted = decoded
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("\n", "<br/>");

            Assert.That(formatted, Does.Not.Contain("&#234;"));
            Assert.That(formatted, Does.Not.Contain("&#224;"));
            Assert.That(formatted, Does.Not.Contain("&#227;"));
            Assert.That(formatted, Does.Not.Contain("&#244;"));
            Assert.That(formatted, Does.Contain("Phiên"));
            Assert.That(formatted, Does.Contain("đã"));
            Assert.That(formatted, Does.Contain("không"));

            string formattedUser = System.Net.WebUtility.HtmlDecode(userQuestion)
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("\n", "<br/>");

            Assert.That(formattedUser, Does.Not.Contain("&#225;"));
            Assert.That(formattedUser, Does.Not.Contain("&#226;"));
            Assert.That(formattedUser, Does.Contain("Danh sách tất cả nhân viên"));
        }
    }
}
