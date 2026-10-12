using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Vector;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiKnowledgeSecurityTests
    {
        private static VectorSecurityFilter Actor() => new VectorSecurityFilter {
            UserId = 5, EffectiveScope = "SELF", AsOf = new DateTimeOffset(2026, 10, 11, 0, 0, 0, TimeSpan.Zero)
        };
        private static JObject Law() => JObject.Parse(@"{
            'domain':'POLICY','tag':'REGULATION','status':'ACTIVE_OFFICIAL','verificationStatus':'OFFICIAL_VERIFIED',
            'tenantId':'GLOBAL','sourceKind':'VIETNAM_LAW','companyId':null,'accessProfile':'PUBLIC_INTERNAL',
            'applicabilityMode':'ALL','appliesToCurrentCompany':true,'effectiveFrom':'2021-01-01T00:00:00Z',
            'effectiveTo':null,'allowedRoleIds':[],'allowedDepartmentIds':[], 'embeddingModel':'bge-m3:latest',
            'text':'Verified synthetic legal evidence', 'clauseId':'PL-01/DIEU-01','totalChunks':1,'chunkIndex':0}");

        [Test] public void GlobalLaw_AllowsAuthenticatedActorWithoutEmployeeOrCompany() {
            Assert.IsTrue(QdrantService.IsPolicyPayloadAuthorized(Law(), Actor()));
        }
        [TestCase("tenantId", "CTY_OTHER")]
        [TestCase("status", "DRAFT_NOT_APPROVED")]
        [TestCase("verificationStatus", "DRAFT_PENDING_CONFIRMATION")]
        [TestCase("effectiveFrom", "2027-01-01T00:00:00Z")]
        [TestCase("effectiveTo", "2026-10-11T00:00:00Z")]
        [TestCase("effectiveFrom", "invalid")]
        [TestCase("accessProfile", "UNKNOWN")]
        [TestCase("applicabilityMode", "UNRESOLVED")]
        public void InvalidOrUnauthorizedMetadata_IsDenied(string field, string value) {
            var payload = Law(); payload[field] = value;
            Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, Actor()));
        }
        [Test] public void MissingEffectiveDate_IsDenied() {
            var payload = Law(); payload.Remove("effectiveFrom");
            Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, Actor()));
        }
        [Test] public void CompanyDocument_CannotMasqueradeAsGlobalLaw() {
            var payload = Law(); payload["sourceKind"] = "COMPANY_POLICY";
            Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, Actor()));
        }
        [Test] public void RestrictedEmptyAcl_DeniesEvenAdmin() {
            var actor = Actor(); actor.IsAdmin = true; actor.EffectiveScope = "ALL";
            var payload = Law(); payload["accessProfile"] = "CONFIDENTIAL";
            Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
        }
        [Test] public void RoleRestriction_RequiresTrustedMatchingMembership() {
            var payload = Law(); payload["accessProfile"] = "ROLE_RESTRICTED"; payload["allowedRoleIds"] = new JArray("HR");
            var actor = Actor(); Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
            actor.AllowedRoleIds.Add("HR"); Assert.IsTrue(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
        }
        [Test] public void DepartmentRestriction_MustMatch() {
            var payload = Law(); payload["accessProfile"] = "CONFIDENTIAL"; payload["allowedDepartmentIds"] = new JArray(3);
            var actor = Actor(); actor.AllowedDepartmentIds.Add(2);
            Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
            actor.AllowedDepartmentIds.Add(3); Assert.IsTrue(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
        }
        [Test] public void ExplicitDeny_WinsOverRoleAndAdmin() {
            var actor = Actor(); actor.IsAdmin = true; actor.EffectiveScope = "ALL"; actor.DenyCodes.Add("POLICY_LOOKUP");
            Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(Law(), actor));
        }
        [Test] public void CompanyTenant_RequiresMatchingCompanyId() {
            var payload = Law(); payload["tenantId"] = "CTY_1"; payload["companyId"] = "CTY_1"; payload["sourceKind"] = "COMPANY_POLICY";
            var actor = Actor(); Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
            actor.AllowedCompanyId = "CTY_1"; Assert.IsTrue(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
            payload["companyId"] = "CTY_2"; Assert.IsFalse(QdrantService.IsPolicyPayloadAuthorized(payload, actor));
        }
        private sealed class Handler : HttpMessageHandler {
            public List<string> Paths = new List<string>(); public JObject SearchBody; public JObject Payload; public JArray ScrollPoints;
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
                Paths.Add(request.RequestUri.AbsolutePath);
                if (request.Method == HttpMethod.Get) return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
                if(request.RequestUri.AbsolutePath.EndsWith("/points/scroll"))
                    return new HttpResponseMessage(HttpStatusCode.OK) {Content=new StringContent(new JObject {
                        ["result"]=new JObject {["points"]=ScrollPoints,["next_page_offset"]=null}}.ToString())};
                SearchBody = JObject.Parse(await request.Content.ReadAsStringAsync());
                var response = new JObject { ["result"] = new JArray(new JObject { ["id"]="synthetic", ["score"]=0.9, ["payload"]=Payload }) };
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response.ToString()) };
            }
        }
        [Test] public async Task PolicyRoute_ChecksKnowledgeAliasAndRejectsUnfilteredExpiredResponse() {
            var payload = Law(); payload["effectiveTo"] = "2025-01-01T00:00:00Z";
            var handler = new Handler { Payload = payload };
            var service = new QdrantService(null, new HttpClient(handler));
            var result = await service.SearchScopedAsync("legal question", new VectorBusinessFilter { Domain="POLICY" }, Actor(), new float[1024]);
            Assert.AreEqual(VectorSearchStatus.NoMatch, result.Status);
            Assert.AreEqual("/collections/hrms_knowledge_active", handler.Paths[0]);
            Assert.AreEqual("/collections/hrms_knowledge_active/points/search", handler.Paths[1]);
            Assert.IsFalse(result.Hits[0].IsAuthorized);
            string filter = handler.SearchBody["filter"].ToString();
            StringAssert.Contains("effectiveFrom", filter); StringAssert.Contains("effectiveTo", filter);
            StringAssert.Contains("accessProfile", filter); StringAssert.Contains("verificationStatus", filter);
            StringAssert.DoesNotContain("employeeId", filter);
        }
        [Test] public async Task PolicyRoute_CanUsePrecomputedEmbeddingForPublicGlobalLaw() {
            var handler = new Handler { Payload=Law() };
            var result = await new QdrantService(null, new HttpClient(handler)).SearchScopedAsync("legal question", new VectorBusinessFilter { Domain="POLICY" }, Actor(), new float[1024]);
            Assert.AreEqual(VectorSearchStatus.Success, result.Status);
        }
        [Test] public async Task PolicyRoute_RejectsIncompatibleEmbeddingDimension() {
            var handler = new Handler { Payload=Law() };
            var result = await new QdrantService(null, new HttpClient(handler)).SearchScopedAsync("legal question", new VectorBusinessFilter { Domain="POLICY" }, Actor(), new float[384]);
            Assert.AreEqual(VectorSearchStatus.InvalidIndex, result.Status);
        }
        [TestCase(false)]
        [TestCase(true)]
        public async Task PolicyExpansion_RequiresCompleteChunksFromExactDocumentVersion(bool wrongSource) {
            var first=Law(); first["sourceId"]="PL-01"; first["version"]="1.1.0"; first["totalChunks"]=2;
            first["text"]="Condition";
            var second=(JObject)first.DeepClone(); second["chunkIndex"]=1; second["text"]="Exception";
            if(wrongSource) second["sourceId"]="PL-OTHER";
            var handler=new Handler {Payload=first,ScrollPoints=new JArray(
                new JObject {["id"]="synthetic",["payload"]=first},
                new JObject {["id"]="exception",["payload"]=second})};
            var result=await new QdrantService(null,new HttpClient(handler)).SearchScopedAsync("question",new VectorBusinessFilter {Domain="POLICY"},Actor(),new float[1024]);
            Assert.AreEqual(wrongSource ? VectorSearchStatus.Unavailable : VectorSearchStatus.Success,result.Status);
            Assert.IsTrue(handler.Paths.Contains("/collections/hrms_knowledge_active/points/scroll"));
            if(!wrongSource) { Assert.AreEqual(2,result.Hits.Count); Assert.AreEqual("Exception",result.Hits[1].Text); }
        }
        [Test] public void EvidenceSelection_KeepsWholeClauseAndSkipsIncompleteChunks() {
            var hits=new List<VectorHit> {
                new VectorHit {PointId="0",ClauseId="A",Text="Condition",Score=0.9f,IsAuthorized=true,TotalChunks=2,ChunkIndex=0},
                new VectorHit {PointId="1",ClauseId="A",Text="Exception",Score=0.8f,IsAuthorized=true,TotalChunks=2,ChunkIndex=1},
                new VectorHit {PointId="2",ClauseId="B",Text="Incomplete",Score=0.7f,IsAuthorized=true,TotalChunks=2,ChunkIndex=0}
            };
            var selected=Bu.Services.AI_Services.Core.AiExecutionService.SelectPolicyEvidence(hits,2000);
            Assert.AreEqual(2,selected.Count); Assert.AreEqual("Exception",selected[1].Text);
            Assert.AreEqual(0,Bu.Services.AI_Services.Core.AiExecutionService.SelectPolicyEvidence(hits,400).Count);
        }
        [Test] public void EvidenceSelection_DoesNotUseFixedTopThree() {
            var hits=new List<VectorHit>();
            for(int i=0;i<5;i++) hits.Add(new VectorHit {PointId=i.ToString(),ClauseId=i.ToString(),Text="Evidence",Score=0.8f,IsAuthorized=true});
            Assert.AreEqual(5,Bu.Services.AI_Services.Core.AiExecutionService.SelectPolicyEvidence(hits,3000).Count);
        }
        [TestCase("Based on [E1] and [E2]",2,true)]
        [TestCase("Invented [E3]",2,false)]
        [TestCase("Invalid [E0]",2,false)]
        public void CitationValidation_OnlyKnownEvidenceIds(string answer,int count,bool valid) {
            Assert.AreEqual(valid,Bu.Services.AI_Services.Core.AiExecutionService.HasOnlyKnownEvidenceCitations(answer,count));
        }
    }
}
