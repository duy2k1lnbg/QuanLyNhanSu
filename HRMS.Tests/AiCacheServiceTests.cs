using NUnit.Framework;
using Bu.Services.AI_Services.Memory;

namespace Bu.Tests
{
    [TestFixture]
    public class AiCacheServiceTests
    {
        private AiCacheService _cacheService;

        [SetUp]
        public void SetUp()
        {
            _cacheService = new AiCacheService();
            _cacheService.Clear();
        }

        [Test]
        public void Set_ShouldStoreValue_AndGet_ShouldRetrieveIt()
        {
            // Arrange
            string question = "Ai sinh nhật tháng này?";
            string sql = "SELECT * FROM V_AI_EMPLOYEE WHERE EXTRACT(MONTH FROM NGAYSINH) = 6";

            // Act
            _cacheService.Set(question, sql);
            string cachedValue = _cacheService.Get(question);

            // Assert
            Assert.AreEqual(sql, cachedValue, "Dữ liệu SQL được lấy từ cache phải khớp với dữ liệu đã lưu.");
        }

        [Test]
        public void Get_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            // Act
            string result = _cacheService.Get("NonExistentQuestion");

            // Assert
            Assert.IsNull(result, "Lấy key không tồn tại phải trả về null.");
        }

        [Test]
        public void Set_ShouldOverwriteExistingValue_WhenKeyIsSame()
        {
            // Arrange
            string question = "Danh sách nhân sự";
            string sql1 = "SELECT * FROM V_AI_EMPLOYEE";
            string sql2 = "SELECT HOTEN FROM V_AI_EMPLOYEE";

            // Act
            _cacheService.Set(question, sql1);
            _cacheService.Set(question, sql2);
            string result = _cacheService.Get(question);

            // Assert
            Assert.AreEqual(sql2, result, "Ghi đè key đã có phải cập nhật giá trị mới nhất.");
        }

        [Test]
        public void Clear_ShouldEmptyTheCache()
        {
            // Arrange
            _cacheService.Set("q1", "sql1");
            _cacheService.Set("q2", "sql2");

            // Act
            _cacheService.Clear();

            // Assert
            Assert.IsNull(_cacheService.Get("q1"));
            Assert.IsNull(_cacheService.Get("q2"));
        }
    }
}