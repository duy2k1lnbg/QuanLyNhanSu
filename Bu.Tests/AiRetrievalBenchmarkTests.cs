using System;
using System.Diagnostics;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Vector;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class AiRetrievalBenchmarkTests
    {
        [Test]
        public void Benchmark_OracleSqlAstValidator_Throughput()
        {
            const int iterations = 5000;
            string testQuery = "SELECT MANV, HOTEN, TEN_PHONGBAN FROM V_AI_EMPLOYEE WHERE UPPER(HOTEN) LIKE '%DUY%' AND MANV > 10";

            // Warmup
            OracleSqlAstValidator.Validate(testQuery);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var result = OracleSqlAstValidator.Validate(testQuery);
                Assert.IsTrue(result.IsValid);
            }
            sw.Stop();

            double opsPerSec = iterations / (sw.ElapsedMilliseconds / 1000.0);
            Console.WriteLine($"[BENCHMARK] OracleSqlAstValidator: {iterations} validations in {sw.ElapsedMilliseconds}ms ({opsPerSec:N0} ops/sec)");

            // AST validation should be very fast (> 1000 validations per second)
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(5000), "AST validation took longer than 5 seconds for 5000 iterations.");
        }

        [Test]
        public void Benchmark_QueryPreprocessor_Latency()
        {
            const int iterations = 3000;
            string rawQuestion = "ai o phong ke toan va luong cao nhat?";

            // Warmup
            QueryPreprocessor.Preprocess(rawQuestion);

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                string processed = QueryPreprocessor.Preprocess(rawQuestion);
                Assert.IsNotEmpty(processed);
            }
            sw.Stop();

            double avgMs = (double)sw.ElapsedMilliseconds / iterations;
            Console.WriteLine($"[BENCHMARK] QueryPreprocessor: {iterations} runs in {sw.ElapsedMilliseconds}ms (Avg: {avgMs:F3}ms/call)");

            Assert.That(avgMs, Is.LessThan(1.0), "Preprocessing must take less than 1ms per query on average.");
        }

        [Test]
        public void Benchmark_AiCacheService_Throughput()
        {
            var cache = new AiCacheService();
            const int iterations = 10000;

            cache.Set("câu hỏi mẫu về nhân sự", "SELECT * FROM V_AI_EMPLOYEE");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                string hit = cache.Get("câu hỏi mẫu về nhân sự");
                Assert.IsNotNull(hit);
            }
            sw.Stop();

            double opsPerSec = iterations / (sw.ElapsedMilliseconds / 1000.0);
            Console.WriteLine($"[BENCHMARK] AiCacheService: {iterations} reads in {sw.ElapsedMilliseconds}ms ({opsPerSec:N0} ops/sec)");

            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000), "Cache lookup took longer than 2 seconds for 10000 operations.");
        }

        [Test]
        public void Benchmark_QdrantOutbox_Enqueue()
        {
            var outbox = QdrantOutboxManager.Instance;
            const int count = 100;

            var sw = Stopwatch.StartNew();
            for (int i = 1; i <= count; i++)
            {
                outbox.EnqueueEmployeeSync(i);
            }
            sw.Stop();

            Console.WriteLine($"[BENCHMARK] QdrantOutbox Enqueue: {count} messages enqueued in {sw.ElapsedMilliseconds}ms");
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(3000), "Enqueueing 100 outbox items should take less than 3 seconds.");
            Assert.That(outbox.PendingCount, Is.GreaterThan(0), "There should be pending items in the outbox queue.");
        }
    }
}
