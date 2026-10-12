using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiRequestTimeoutTests
    {
        private class Prompts : IPromptManager {
            public string GetSchema() => "";
            public string GetPrompt(string key) => "{Context} {History} {Question}";
        }
        private class DelayedHandler : HttpMessageHandler {
            public bool CancellationObserved;
            public string RequestBody;
            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
                RequestBody = await request.Content.ReadAsStringAsync();
                try { await Task.Delay(100,cancellationToken); }
                catch (OperationCanceledException) { CancellationObserved=true; throw; }
                return new HttpResponseMessage(HttpStatusCode.OK) { Content=new StringContent("{\"model\":\"qwen2.5:latest\",\"response\":\"Phản hồi kiểm thử\",\"embedding\":[1.0,2.0]}") };
            }
        }
        [TestCase(null,180)] [TestCase(0,180)] [TestCase(29,180)]
        [TestCase(30,30)] [TestCase(180,180)] [TestCase(300,300)] [TestCase(900,900)] [TestCase(901,180)]
        public void InvalidStoredValuesUseDefaultAndValidValuesArePreserved(int? input,int expected) {
            Assert.AreEqual(expected,AiConfigurationSnapshot.NormalizeRequestTimeout(input));
            Assert.Greater(AiConfigurationSnapshot.ClientTransportTimeoutSeconds,AiConfigurationSnapshot.MaxRequestTimeoutSeconds);
        }
        [Test] public void ClonePreservesConfiguredTimeoutWithoutSharingState() {
            var original=new AiConfigurationSnapshot {AiRequestTimeoutSeconds=300};var clone=original.Clone();
            Assert.AreEqual(300,clone.AiRequestTimeoutSeconds);clone.AiRequestTimeoutSeconds=900;
            Assert.AreEqual(300,original.AiRequestTimeoutSeconds);
        }
        [TestCase(0)] [TestCase(29)] [TestCase(901)]
        public async Task InvalidUpdateIsRejectedBeforeOpeningDatabaseTransaction(int timeout) {
            int dbCalls=0;
            var coordinator=new AiConfigurationCoordinator(()=>{dbCalls++;throw new InvalidOperationException("Synthetic DB unavailable");});
            var result=await coordinator.UpdateConfigurationAsync(new AiConfigUpdateDto {ExpectedVersion=1,AiRequestTimeoutSeconds=timeout},1);
            Assert.False(result.Success);Assert.That(result.Message,Does.Contain("30 đến 900"));Assert.AreEqual(1,dbCalls);
        }
        [Test] public async Task OllamaResponseAndPinnedModelRemainCorrectWithLongerBudget() {
            var handler=new DelayedHandler();
            using(var client=new HttpClient(handler)) {
                var service=new OllamaService(new Prompts(),(k,f)=>f,client,()=>new AiConfigurationSnapshot {AiRequestTimeoutSeconds=300});
                Assert.AreEqual("Phản hồi kiểm thử",await service.GenerateGeneralChatAsync("Xin chào"));
                Assert.That(handler.RequestBody,Does.Contain("qwen2.5:latest"));
            }
        }
        [TestCase("chat")] [TestCase("json")] [TestCase("embedding")]
        public async Task CallerCanStillCancelAllOllamaRequestKinds(string kind) {
            var handler=new DelayedHandler();
            using(var client=new HttpClient(handler))
            using(var cts=new CancellationTokenSource()) {
                var service=new OllamaService(new Prompts(),(k,f)=>f,client,()=>new AiConfigurationSnapshot {AiRequestTimeoutSeconds=900});
                Task work=kind=="chat" ? (Task)service.GenerateGeneralChatAsync("Xin chào",cancellationToken:cts.Token)
                    : kind=="json" ? (Task)service.AskStructuredJsonAsync("{}","test",cts.Token)
                    : service.GetEmbedding("test",cts.Token);
                cts.Cancel();await work;Assert.True(handler.CancellationObserved);
            }
        }
        [Test] public void SharedOllamaTransportDoesNotImposeItsOldTwoMinuteLimit() {
            var field=typeof(OllamaService).GetField("SharedClient",BindingFlags.Static|BindingFlags.NonPublic);
            Assert.AreEqual(Timeout.InfiniteTimeSpan,((HttpClient)field.GetValue(null)).Timeout);
        }
    }
}
