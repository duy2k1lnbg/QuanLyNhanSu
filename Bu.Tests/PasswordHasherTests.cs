using NUnit.Framework;
using Bu.CLASS_SYSTEM;

namespace Bu.Tests
{
    [TestFixture]
    public class PasswordHasherTests
    {
        [Test]
        public void HashPassword_ShouldReturnBCryptHash_WhenPasswordIsValid()
        {
            // Arrange
            string password = "MySecurePassword123";

            // Act
            string hash = PasswordHasher.HashPassword(password);

            // Assert
            Assert.IsNotEmpty(hash);
            Assert.IsTrue(hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$"), 
                "Băm mật khẩu phải định dạng BCrypt.");
        }

        [Test]
        public void HashPassword_ShouldReturnEmptyString_WhenPasswordIsNullOrEmpty()
        {
            // Act & Assert
            Assert.IsEmpty(PasswordHasher.HashPassword(null));
            Assert.IsEmpty(PasswordHasher.HashPassword(string.Empty));
        }

        [Test]
        public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            // Arrange
            string password = "AdminPassword";
            string hash = PasswordHasher.HashPassword(password);

            // Act
            bool result = PasswordHasher.VerifyPassword(password, hash);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            // Arrange
            string password = "AdminPassword";
            string hash = PasswordHasher.HashPassword(password);

            // Act
            bool result = PasswordHasher.VerifyPassword("WrongPassword", hash);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyPassword_ShouldReturnTrue_WhenMatchingLegacyPlainText()
        {
            // Arrange
            string plainText = "LegacyPassword";

            // Act
            bool result = PasswordHasher.VerifyPassword(plainText, plainText);

            // Assert
            Assert.IsTrue(result, "Phải hỗ trợ đối khớp trực tiếp với mật khẩu thô cũ.");
        }

        [Test]
        public void VerifyPassword_ShouldTrimFixedLengthPadding()
        {
            // Arrange
            string plainText = "Password";
            string paddedText = "Password      "; // Simulation of Oracle fixed CHAR(20) type padding

            // Act
            bool result = PasswordHasher.VerifyPassword(paddedText, plainText);

            // Assert
            Assert.IsTrue(result, "Phải tự động trim khoảng trắng thừa ở đuôi mật khẩu.");
        }

        [Test]
        public void VerifyPassword_ShouldReturnFalse_WhenInputsAreNullOrEmpty()
        {
            // Act & Assert
            Assert.IsFalse(PasswordHasher.VerifyPassword(null, "hash"));
            Assert.IsFalse(PasswordHasher.VerifyPassword("password", null));
            Assert.IsFalse(PasswordHasher.VerifyPassword("", ""));
        }
    }
}