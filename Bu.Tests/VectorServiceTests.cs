using NUnit.Framework;
using Bu.Services.AI_Services;
using Bu.Services.AI_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bu.Tests
{
    public class FakeLlmService : ILlmService
    {
        public Task<string> AskSql(string prompt) => Task.FromResult("SELECT * FROM V_AI_EMPLOYEE");
        public Task<string> AskChat(string context, string question, string history, Action<string> onTokenReceived = null) => Task.FromResult("Hello");
        public Task<float[]> GetEmbedding(string text, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult<float[]>(null);
    }

    [TestFixture]
    public class VectorServiceTests
    {
        private VectorService _vectorService;

        [SetUp]
        public void SetUp()
        {
            _vectorService = new VectorService(new FakeLlmService());
        }

        [Test]
        public void Add_ShouldNotAddNullOrEmptyText()
        {
            _vectorService.Add("");
            _vectorService.Add(null);
            
            var results = _vectorService.Search("test");
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Search_ShouldReturnEmpty_WhenQueryIsEmpty()
        {
            _vectorService.Add("Nhân viên Nguyễn Thọ Duy thuộc phòng ban Kế toán", "EMPLOYEE");
            
            var results = _vectorService.Search("");
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void Search_ShouldMatchExactOrSimilarPhrase()
        {
            _vectorService.Add("Nhân viên Nguyễn Thọ Duy thuộc phòng ban Kế toán", "EMPLOYEE");
            _vectorService.Add("Nhân viên Trần Thị B có số bảo hiểm BH12345", "INSURANCE");

            var employeeResults = _vectorService.Search("Nguyễn Thọ Duy", "EMPLOYEE");
            Assert.IsTrue(employeeResults.Count > 0);
            Assert.IsTrue(employeeResults[0].Contains("Nguyễn Thọ Duy"));

            var insuranceResults = _vectorService.Search("bảo hiểm của Trần Thị B", "INSURANCE");
            Assert.IsTrue(insuranceResults.Count > 0);
            Assert.IsTrue(insuranceResults[0].Contains("BH12345"));
        }

        [Test]
        public void Search_ShouldFilterByTag()
        {
            _vectorService.Add("Nhân viên Nguyễn Thọ Duy thuộc phòng ban Kế toán", "EMPLOYEE");
            _vectorService.Add("Nhân viên Trần Thị B nhận phụ cấp ăn trưa", "ALLOWANCE");

            // Search with tag EMPLOYEE
            var resultsWithTag = _vectorService.Search("Trần Thị B", "EMPLOYEE");
            Assert.AreEqual(0, resultsWithTag.Count);

            // Search with tag ALLOWANCE
            var resultsWithCorrectTag = _vectorService.Search("Trần Thị B", "ALLOWANCE");
            Assert.IsTrue(resultsWithCorrectTag.Count > 0);
        }

        [Test]
        public void Search_ShouldFallbackToLowerThreshold_WhenNoHighMatchFound()
        {
            _vectorService.Add("Nhân viên công ty HRMS chuyên nghiệp làm việc tại Hà Nội", "GENERAL");

            // Query with medium similarity
            var results = _vectorService.Search("làm việc tại Hà Nội", "GENERAL");
            Assert.IsTrue(results.Count > 0, "Should match via fallback threshold.");
        }
    }
}
