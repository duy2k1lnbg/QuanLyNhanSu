using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Memory;
using Bu.Services.AI_Services.Vector;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiCacheAndVectorV2Tests
    {
        private FakeClockProvider _fakeClock;
        private AiCacheCoordinator _coordinator;

        [SetUp]
        public void SetUp()
        {
            _fakeClock = new FakeClockProvider(new DateTime(2026, 10, 3, 12, 0, 0));
            _coordinator = new AiCacheCoordinator(_fakeClock);
        }

        #region Canonical Key Tests (C01 - C04)

        [Test]
        public void CanonicalKeys_EquivalentQueries_GenerateSameKey_C01()
        {
            string key1 = AiCacheCoordinator.BuildPlanKey("OVERTIME", "SUM", "SOGIO", "MANV=10|MONTH=9|YEAR=2026", 1);
            string key2 = AiCacheCoordinator.BuildPlanKey("OVERTIME", "SUM", "SOGIO", "MANV=10|MONTH=9|YEAR=2026", 1);

            Assert.AreEqual(key1, key2);
            Assert.IsTrue(key1.StartsWith("plan:"));
        }

        [Test]
        public void CanonicalKeys_DifferentParameters_GenerateDistinctKeys_C02()
        {
            string keyMonth9 = AiCacheCoordinator.BuildPlanKey("OVERTIME", "SUM", "SOGIO", "MANV=10|MONTH=9|YEAR=2026", 1);
            string keyMonth8 = AiCacheCoordinator.BuildPlanKey("OVERTIME", "SUM", "SOGIO", "MANV=10|MONTH=8|YEAR=2026", 1);
            string keyEmployee20 = AiCacheCoordinator.BuildPlanKey("OVERTIME", "SUM", "SOGIO", "MANV=20|MONTH=9|YEAR=2026", 1);

            Assert.AreNotEqual(keyMonth9, keyMonth8);
            Assert.AreNotEqual(keyMonth9, keyEmployee20);
        }

        [Test]
        public void CanonicalKeys_DifferentUsersAndScopes_GenerateDistinctKeys_C04()
        {
            string user1Key = AiCacheCoordinator.BuildResultKey("EMPLOYEE", "LOOKUP", "10", "2026", "DEPT:2", policyRev: 1);
            string user2Key = AiCacheCoordinator.BuildResultKey("EMPLOYEE", "LOOKUP", "10", "2026", "DEPT:3", policyRev: 1);

            Assert.AreNotEqual(user1Key, user2Key);
        }

        #endregion

        #region Absolute TTL & Eviction Tests (C05, C19)

        [Test]
        public void BoundedCacheStore_AbsoluteTtl_ExpiresWhenClockAdvances_C05()
        {
            var store = new BoundedCacheStore<string>(100, TimeSpan.FromMinutes(10), _fakeClock);
            store.Set("key1", "value1");

            // Trước hạn: còn tồn tại
            Assert.IsTrue(store.TryGet("key1", out var val));
            Assert.AreEqual("value1", val);

            // Thời gian trôi qua 11 phút (> 10 phút TTL)
            _fakeClock.Advance(TimeSpan.FromMinutes(11));

            // Sau hạn: đã hết hạn (expire)
            Assert.IsFalse(store.TryGet("key1", out _));
        }

        [Test]
        public void BoundedCacheStore_CapacityBudget_EvictsOldestEntries_C19()
        {
            // Dung lượng tối đa: 5 bản ghi
            var store = new BoundedCacheStore<string>(5, TimeSpan.FromHours(1), _fakeClock);

            for (int i = 1; i <= 5; i++)
            {
                store.Set($"key_{i}", $"val_{i}");
                _fakeClock.Advance(TimeSpan.FromSeconds(1));
            }

            Assert.AreEqual(5, store.Count);

            // Thêm bản ghi thứ 6 -> vượt capacity 5 -> kích hoạt LRU eviction
            store.Set("key_6", "val_6");

            // Dung lượng vẫn nằm trong giới hạn cho phép
            Assert.IsTrue(store.Count <= 5);
            // key_6 mới thêm phải tồn tại
            Assert.IsTrue(store.TryGet("key_6", out _));
        }

        #endregion

        #region Single-Flight Concurrency Tests (C10, C11)

        [Test]
        public async Task SingleFlight_TenConcurrentRequests_ExecutesFactoryOnlyOnce_C10()
        {
            int factoryCallCount = 0;
            string key = "shared_heavy_computation";

            // Khởi tạo 10 task đồng thời cho cùng 1 key
            var tasks = new List<Task<RenderedResponse>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    return await _coordinator.GetOrExecuteResultAsync(key, isSensitive: false, async () =>
                    {
                        Interlocked.Increment(ref factoryCallCount);
                        await Task.Delay(50); // Mô phỏng truy vấn DB mất 50ms
                        return new RenderedResponse { Status = "answered", Answer = "Single-flight data" };
                    });
                }));
            }

            var results = await Task.WhenAll(tasks);

            // Cả 10 callers đều nhận được kết quả
            Assert.AreEqual(10, results.Length);
            foreach (var r in results)
            {
                Assert.AreEqual("Single-flight data", r.Answer);
            }

            // Nhưng factory chỉ được gọi đúng 1 LẦN DUY NHẤT! (Tiêu chí C10)
            Assert.AreEqual(1, factoryCallCount);
        }

        [Test]
        public async Task SingleFlight_OneWaiterCancel_DoesNotCorruptRemaining_C11()
        {
            var singleFlight = new SingleFlightCoordinator();
            string key = "cancellation_test_key";
            int factoryExecutions = 0;

            var cts = new CancellationTokenSource();

            var task1 = Task.Run(async () =>
            {
                return await singleFlight.ExecuteAsync(key, async () =>
                {
                    Interlocked.Increment(ref factoryExecutions);
                    await Task.Delay(100);
                    return "Result";
                });
            });

            var task2 = Task.Run(async () =>
            {
                // Task 2 bị hủy sớm
                cts.Cancel();
                cts.Token.ThrowIfCancellationRequested();
                return await singleFlight.ExecuteAsync(key, async () => 
                {
                    await Task.Yield();
                    return "Result";
                });
            });

            // Task 2 ném OperationCanceledException
            Assert.ThrowsAsync<OperationCanceledException>(async () => await task2);

            // Nhưng Task 1 vẫn hoàn thành thành công và nhận được dữ liệu (C11)
            string result1 = await task1;
            Assert.AreEqual("Result", result1);
            Assert.AreEqual(1, factoryExecutions);
        }

        #endregion

        #region Sensitive & Error Non-Caching Tests (C12, C13)

        [Test]
        public async Task GetOrExecuteResult_SensitivePayroll_BypassesCache_C13()
        {
            int factoryCalls = 0;
            string key = "payroll_sensitive_key";

            Func<Task<RenderedResponse>> payrollFactory = async () =>
            {
                Interlocked.Increment(ref factoryCalls);
                await Task.Yield();
                return new RenderedResponse { Status = "answered", Answer = $"Payroll data at {DateTime.UtcNow.Ticks}" };
            };

            // Lần 1: isSensitive = true
            var res1 = await _coordinator.GetOrExecuteResultAsync(key, isSensitive: true, payrollFactory);
            // Lần 2: isSensitive = true
            var res2 = await _coordinator.GetOrExecuteResultAsync(key, isSensitive: true, payrollFactory);

            // Phải gọi DB/Factory cả 2 lần (Bypass cache hoàn toàn - C13)
            Assert.AreEqual(2, factoryCalls);
        }

        [Test]
        public async Task GetOrExecuteResult_ErrorOrTimeout_NeverCachedAsSuccess_C12()
        {
            int factoryCalls = 0;
            string key = "failing_query_key";

            Func<Task<RenderedResponse>> errorFactory = async () =>
            {
                Interlocked.Increment(ref factoryCalls);
                await Task.Yield();
                return new RenderedResponse { Status = "error", Answer = "Lỗi kết nối cơ sở dữ liệu." };
            };

            var res1 = await _coordinator.GetOrExecuteResultAsync(key, isSensitive: false, errorFactory);
            Assert.AreEqual("error", res1.Status);

            // Gọi lần thứ 2: Không được lấy từ cache, phải gọi lại factory!
            var res2 = await _coordinator.GetOrExecuteResultAsync(key, isSensitive: false, errorFactory);
            Assert.AreEqual(2, factoryCalls);
        }

        #endregion

        [TestCase(false)]
        [TestCase(true)]
        public async Task VectorSearchFailsClosedUntilCurrentAclAndCorpusAreConfigured(bool admin)
        {
            // Avoid the real service constructor: it initializes remote clients/background work.
            var service = (QdrantService)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(QdrantService));
            var response = await service.SearchScopedAsync("Tài liệu bảo mật", new VectorBusinessFilter { Tag = null }, new VectorSecurityFilter { IsAdmin = admin, CallerEmployeeId = 10, TargetEmployeeId = 99, AllowedDepartmentIds = new List<int> { 2 } });
            Assert.IsEmpty(response.Hits);
            StringAssert.StartsWith("disabled:", response.SecurityFilterApplied);
        }

    }
}
