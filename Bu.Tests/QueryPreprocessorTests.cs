using NUnit.Framework;
using Bu.Services.AI_Services.Core;

namespace Bu.Tests
{
    [TestFixture]
    public class QueryPreprocessorTests
    {
        [Test]
        public void Preprocess_ShouldNormalizeUnaccentedKeywords()
        {
            // Arrange
            string input = "tim nhan vien ke toan";

            // Act
            string result = QueryPreprocessor.Preprocess(input);

            // Assert
            Assert.IsTrue(result.Contains("nhân viên"));
            Assert.IsTrue(result.Contains("kế toán"));
        }

        [Test]
        public void Preprocess_ShouldInjectBirthdayHint_WhenQuestionRelatesToBirthday()
        {
            // Arrange
            string input = "nhan vien nao sinh nhat trong thang nay";

            // Act
            string result = QueryPreprocessor.Preprocess(input);

            // Assert
            Assert.IsTrue(result.Contains("nhân viên"));
            Assert.IsTrue(result.Contains("sinh nhật"));
            Assert.IsTrue(result.Contains("Gợi ý CSDL: Để lọc sinh nhật trong tháng hiện tại"), 
                "Phải inject gợi ý CSDL về sinh nhật nhân viên.");
        }

        [Test]
        public void Preprocess_ShouldInjectOvertimeHint_WhenQuestionRelatesToOvertime()
        {
            // Arrange
            string input = "tong so gio tang ca";

            // Act
            string result = QueryPreprocessor.Preprocess(input);

            // Assert
            Assert.IsTrue(result.Contains("tăng ca"));
            Assert.IsTrue(result.Contains("TB_TANGCA"), "Phải inject gợi ý bảng tăng ca.");
        }

        [Test]
        public void Preprocess_ShouldInjectAllowanceHint_WhenQuestionRelatesToAllowance()
        {
            // Arrange
            string input = "cac loai phu cap cua nhan vien";

            // Act
            string result = QueryPreprocessor.Preprocess(input);

            // Assert
            Assert.IsTrue(result.Contains("phụ cấp"));
            Assert.IsTrue(result.Contains("TB_NHANVIEN_PHUCAP"), "Phải gợi ý bảng phụ cấp.");
        }

        [Test]
        public void Preprocess_ShouldInjectInsuranceHint_WhenQuestionRelatesToInsurance()
        {
            // Arrange
            string input = "so bao hiem cua nhan vien";

            // Act
            string result = QueryPreprocessor.Preprocess(input);

            // Assert
            Assert.IsTrue(result.Contains("bảo hiểm"));
            Assert.IsTrue(result.Contains("TB_BAOHIEM"), "Phải gợi ý bảng bảo hiểm.");
        }

        [Test]
        public void Preprocess_ShouldReturnOriginalString_WhenInputIsNullOrEmpty()
        {
            // Act & Assert
            Assert.IsNull(QueryPreprocessor.Preprocess(null));
            Assert.IsEmpty(QueryPreprocessor.Preprocess(string.Empty));
            Assert.AreEqual("   ", QueryPreprocessor.Preprocess("   "));
        }
    }
}