using System;
using System.Linq;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Vector;
using NUnit.Framework;
namespace HRMS.Tests {
    [TestFixture]
    public class AiKnowledgeLiveIntegrationTests {
        [SetUp] public void RequireOptIn() {
            if (Environment.GetEnvironmentVariable("HRMS_QDRANT_INTEGRATION") != "1") Assert.Ignore("Local Ollama/Qdrant integration requires opt-in.");
        }
        private QdrantService Service() => new QdrantService(new OllamaService(null,
            (key,fallback)=>fallback, snapshotReader:()=>new AiConfigurationSnapshot {
                OllamaHost="http://127.0.0.1:11434", AiRequestTimeoutSeconds=60
            }));
        private VectorSecurityFilter Actor() => new VectorSecurityFilter {UserId=5,EffectiveScope="SELF",AsOf=DateTimeOffset.UtcNow};
        [Test] public async Task ConfiguredEmbeddingModelAndPublishedAlias_ReturnVerifiedLawWithoutCompanyOrEmployee() {
            var result=await Service().SearchScopedAsync("Thời giờ làm việc bình thường theo Bộ luật lao động quy định thế nào?",new VectorBusinessFilter {Domain="POLICY"},Actor());
            Assert.AreEqual(VectorSearchStatus.Success,result.Status,result.ErrorMessage);
            Assert.IsTrue(result.Hits.Any(h=>h.IsAuthorized && h.ClauseId=="PL-01/DIEU-01"));
            Assert.IsTrue(result.Hits.Where(h=>h.IsAuthorized).All(h=>h.TenantId=="GLOBAL" && !string.IsNullOrWhiteSpace(h.DocumentTitle)));
        }
        [Test] public async Task HistoricalAsOfBeforeLawEffectiveDate_ReturnsNoMatch() {
            var actor=Actor(); actor.AsOf=new DateTimeOffset(2000,1,1,0,0,0,TimeSpan.Zero);
            var result=await Service().SearchScopedAsync("Thời giờ làm việc bình thường",new VectorBusinessFilter {Domain="POLICY"},actor);
            Assert.AreEqual(VectorSearchStatus.NoMatch,result.Status,result.ErrorMessage);
        }
        [Test] public async Task OutsideCorpus_DoesNotReturnUnrelatedLowScoreLaw() {
            var result=await Service().SearchScopedAsync("Quy định về phụ cấp nguy hiểm trong lò phản ứng hạt nhân của công ty?",new VectorBusinessFilter {Domain="POLICY"},Actor());
            Assert.AreEqual(VectorSearchStatus.NoMatch,result.Status,result.ErrorMessage);
        }
    }
}
