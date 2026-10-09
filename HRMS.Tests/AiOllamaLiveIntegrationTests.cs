using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HRMS.Tests
{
    // Run only with HRMS_OLLAMA_LIVE_TESTS=1 and an explicit TestCategory=OllamaLive filter.
    // Synthetic fixtures only; no Oracle queries, writes, corpus ingestion or Qdrant calls.
    [TestFixture, Category("OllamaLive"), NonParallelizable]
    public class AiOllamaLiveIntegrationTests
    {
        private const string Host = "http://127.0.0.1:11434";
        private const string PrimaryModel = "qwen2.5:latest";
        private readonly List<string> models = new List<string>();

        private class SyntheticPromptManager : IPromptManager
        {
            public string GetSchema() => "";
            public string GetPrompt(string key) => key == "ChatPromptTemplate"
                ? "DỮ LIỆU GIẢ LẬP ĐƯỢC PHÉP DÙNG:\n{Context}\nLỊCH SỬ:\n{History}\nCÂU HỎI:\n{Question}\nChỉ dựa vào dữ liệu trên, trả lời một câu tiếng Việt. Không tự thêm người, kỳ hay số tiền/giờ."
                : "";
        }
        private static OllamaService Create(string model = PrimaryModel) => new OllamaService(new SyntheticPromptManager(),
            (key,fallback) => key == "OllamaHost" ? Host
                : key == "AiModel" ? model
                : key == "AiTemp" ? "0"
                : key == "AiMaxTokens" ? "120"
                : key == "AiCtx" ? "3072"
                : fallback);

        [OneTimeSetUp]
        public async Task RequireExplicitLocalRunAndInstalledModels()
        {
            if (Environment.GetEnvironmentVariable("HRMS_OLLAMA_LIVE_TESTS") != "1")
                Assert.Ignore("Opt-in local model tests. Set HRMS_OLLAMA_LIVE_TESTS=1 and select category OllamaLive.");
            using (var client=new HttpClient {Timeout=TimeSpan.FromSeconds(10)})
            {
                var version=JObject.Parse(await client.GetStringAsync(Host+"/api/version"));
                var tags=JObject.Parse(await client.GetStringAsync(Host+"/api/tags"));
                models.AddRange(tags["models"].Select(m=>(string)m["name"]));
                TestContext.Progress.WriteLine("Ollama version="+version["version"]+"; installed="+string.Join(",",models));
                Assert.That(models,Does.Contain(PrimaryModel));
                Assert.That(models,Does.Contain("bge-m3:latest"));
            }
        }
        private static async Task<string> Timed(string name,Func<Task<string>> call)
        {
            var sw=Stopwatch.StartNew();var response=await call();
            TestContext.Progress.WriteLine(name+" elapsed_ms="+sw.ElapsedMilliseconds+" response="+response);
            Assert.That(response,Is.Not.Null.And.Not.Empty);
            Assert.That(response,Does.Not.Contain("Lỗi kết nối dịch vụ AI").And.Not.Contain("Hệ thống AI đang bận"));
            return response;
        }

        [Test]
        public async Task PrimaryModel_ClassifiesOvertimeWithRealTransport()
        {
            var response=await Timed("intent/7b",()=>Create().AskIntent(
                "Phân loại vào đúng một nhãn: EMPLOYEE, ATTENDANCE, OVERTIME, INSURANCE, ADVANCE, ALLOWANCE, GENERAL.\nCâu hỏi: Tổng số giờ làm thêm của mã nhân viên 10 tháng 9 năm 2026?\nChỉ in nhãn:"));
            Assert.AreEqual("OVERTIME",response.Trim().TrimEnd('.').ToUpperInvariant());
        }

        [Test]
        public async Task PrimaryModel_GeneratesViewOnlySqlButDoesNotExecuteIt()
        {
            var response=await Timed("sql/7b",()=>Create().AskSql(
                "Schema được duyệt: HR.V_AI_OVERTIME_SUMMARY(MANV NUMBER,THANG NUMBER,NAM NUMBER,SOGIO NUMBER).\n"+
                "Câu hỏi: Nhân viên mã 10 làm thêm tổng bao nhiêu giờ trong tháng 9 năm 2026?\n"+
                "Dùng bind :p_manv cho mã nhân viên, :p_thang cho tháng, :p_nam cho năm; không chèn giá trị literal.\n"+
                "Đặt alias kết quả TONG_SOGIO và trả 0 khi tổng là NULL. Chỉ dùng view và cột được nêu; chỉ trả SELECT, không giải thích."));
            var validated=OracleSqlAstValidator.Validate(response);
            Assert.True(validated.IsValid,validated.RejectionReason);
            Assert.That(validated.ReferencedTables,Does.Contain("HR.V_AI_OVERTIME_SUMMARY").Or.Contain("V_AI_OVERTIME_SUMMARY"));
            StringAssert.Contains(":p_manv",validated.CleanedSql);StringAssert.Contains(":p_thang",validated.CleanedSql);StringAssert.Contains(":p_nam",validated.CleanedSql);
            Assert.True(Regex.IsMatch(validated.CleanedSql,@"(?i)\bSUM\s*\(\s*SOGIO\s*\)"));
            foreach(var pair in new[]{"MANV:p_manv","THANG:p_thang","NAM:p_nam"})
            {
                var parts=pair.Split(':');
                Assert.True(Regex.IsMatch(validated.CleanedSql,@"(?i)\b"+parts[0]+@"\s*=\s*:"+parts[1]+@"\b"),"Missing correct filter "+pair);
            }
        }

        [Test]
        public async Task PrimaryModel_SummarizesSyntheticAuthorizedContext()
        {
            var response=await Timed("chat/7b",()=>Create().AskChat(
                "DỮ LIỆU GIẢ LẬP: Nhân viên thử nghiệm A (#10), kỳ 09/2026, tổng tăng ca 24,5 giờ.",
                "Nhân viên thử nghiệm A tăng ca tổng bao nhiêu giờ trong kỳ 09/2026?",""));
            Assert.True(Regex.IsMatch(response,@"24[.,]5"),response);
        }

        [Test]
        public async Task PrimaryModel_StreamingTokensMatchFinalAnswer()
        {
            var tokens=new List<string>();
            var response=await Timed("stream/7b",()=>Create().AskChat(
                "DỮ LIỆU GIẢ LẬP: Nhân viên thử nghiệm A (#10), kỳ 09/2026, tổng tăng ca 24,5 giờ.",
                "Chỉ trả lời tổng số giờ tăng ca trong dữ liệu.","",token=>tokens.Add(token)));
            Assert.That(tokens.Count,Is.GreaterThan(0));
            Assert.AreEqual(response,string.Concat(tokens).Trim());
            Assert.True(Regex.IsMatch(response,@"24[.,]5"),response);
        }

        [Test]
        public async Task SmallerInstalledModel_ClassifiesWithSameTransport()
        {
            if(!models.Contains("qwen2.5:3b")) Assert.Ignore("Optional 3b model is not installed.");
            var response=await Timed("intent/3b",()=>Create("qwen2.5:3b").AskIntent(
                "Chỉ chọn một nhãn: EMPLOYEE, ATTENDANCE, OVERTIME, INSURANCE, ADVANCE, ALLOWANCE, GENERAL.\nCâu hỏi: Tổng giờ làm thêm của mã 10 trong tháng 9 năm 2026?\nNhãn:"));
            Assert.AreEqual("OVERTIME",response.Trim().TrimEnd('.').ToUpperInvariant());
        }

        private static double Cosine(float[] a,float[] b)
        {
            Assert.AreEqual(a.Length,b.Length);
            return a.Zip(b,(x,y)=>(double)x*y).Sum()/Math.Sqrt(a.Sum(x=>(double)x*x)*b.Sum(x=>(double)x*x));
        }

        [Test]
        public async Task Embedding_Has1024FiniteDimensionsAndStableRepeatedInput()
        {
            var service=Create();var sw=Stopwatch.StartNew();
            var a=await service.GetEmbedding("Nhân viên làm thêm ngoài giờ trong tháng.");
            var b=await service.GetEmbedding("Nhân viên làm thêm ngoài giờ trong tháng.");
            Assert.IsNotNull(a);Assert.IsNotNull(b);Assert.AreEqual(1024,a.Length);
            Assert.True(a.All(x=>!float.IsNaN(x)&&!float.IsInfinity(x)));
            Assert.That(a.Sum(x=>(double)x*x),Is.GreaterThan(0));
            double similarity=Cosine(a,b);
            TestContext.Progress.WriteLine("embedding repeat elapsed_ms="+sw.ElapsedMilliseconds+" dimensions="+a.Length+" cosine="+similarity);
            Assert.That(similarity,Is.GreaterThan(0.999));
        }

        [Test]
        public async Task Embedding_VietnameseMeaningRanksRelatedTextAboveUnrelatedText()
        {
            var service=Create();var sw=Stopwatch.StartNew();
            var query=await service.GetEmbedding("Tổng số giờ làm thêm của nhân viên tháng trước.");
            var related=await service.GetEmbedding("Báo cáo tổng giờ tăng ca của người lao động trong kỳ tháng trước.");
            var unrelated=await service.GetEmbedding("Hướng dẫn nấu phở bò và lựa chọn gia vị.");
            Assert.IsNotNull(query);Assert.IsNotNull(related);Assert.IsNotNull(unrelated);
            double positive=Cosine(query,related),negative=Cosine(query,unrelated);
            TestContext.Progress.WriteLine("embedding semantic elapsed_ms="+sw.ElapsedMilliseconds+" related="+positive+" unrelated="+negative);
            Assert.That(positive,Is.GreaterThan(negative));
        }

        [Test]
        public async Task MissingModel_ReturnsConnectionErrorWithoutPretendingToAnswer()
        {
            var response=await Create("hrms-nonexistent-test-model-"+Guid.NewGuid().ToString("N")).AskIntent("Chỉ trả OVERTIME");
            Assert.AreEqual("Lỗi kết nối dịch vụ AI.",response);
        }

        [Test]
        public async Task CancelledEmbedding_DoesNotReturnSuccessfulVector()
        {
            using(var cancelled=new CancellationTokenSource())
            {
                cancelled.Cancel();
                var vector=await Create().GetEmbedding("Không được gửi nội dung này khi đã hủy.",cancelled.Token);
                Assert.IsNull(vector);
            }
        }
    }
}