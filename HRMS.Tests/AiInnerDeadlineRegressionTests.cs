using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Core;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Memory;
using Bu.Tests;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class AiInnerDeadlineRegressionTests
    {
        private sealed class SlowLlm : ILlmService {
            public int Calls;
            public int DelayMs=11000;
            public async Task<string> GenerateGeneralChatAsync(string q,string h=null,CancellationToken cancellationToken=default) {
                Calls++;await Task.Delay(DelayMs,cancellationToken);return "Xin chào! Tôi có thể hỗ trợ bạn tra cứu thông tin theo quyền được cấp.";
            }
            public Task<string> GenerateGroundedAnswerAsync(string q,string f,string h=null,CancellationToken cancellationToken=default)=>Task.FromResult("Có 15 nhân viên trong phạm vi hiện tại.");
            public Task<string> AskSql(string p)=>Task.FromResult<string>(null);
            public Task<string> AskIntent(string p)=>Task.FromResult<string>(null);
            public Task<string> AskChat(string c,string q,string h,Action<string> a=null)=>Task.FromResult<string>(null);
            public Task<float[]> GetEmbedding(string t,CancellationToken ct=default)=>Task.FromResult(new float[0]);
            public Task<string> AskStructuredJsonAsync(string p,string s,CancellationToken ct=default)=>Task.FromResult<string>(null);
        }
        private static AiExecutionService Service(SlowLlm llm,Func<int> timeoutReader) {
            var clock=new FakeClockProvider(new DateTime(2026,10,11,12,0,0));
            return new AiExecutionService(new QueryUnderstandingService(clock,new EntityResolver(new FakeEntityLookupProvider())),
                sqlExecutor:new RegressionSqlExecutor(),conversationManager:new ConversationStateManager(clock),
                cacheCoordinator:new AiCacheCoordinator(clock),clock:clock,llmService:llm,requestTimeoutReader:timeoutReader);
        }
        [Test, Timeout(15000)]
        public async Task ExactReportedGreetingCanCompleteAfterOldTenSecondLimit() {
            var llm=new SlowLlm();int reads=0;
            var result=await Service(llm,()=>{reads++;return 30;}).ProcessChatAsync(
                "xin chào, bạn có thể giúp tôi không",AiTestContexts.All(),"slow-general");
            Assert.AreEqual("answered",result.Status);Assert.AreEqual(200,result.HttpStatus);
            Assert.AreEqual(1,llm.Calls);Assert.Greater(reads,0);Assert.False(result.BypassedLlm);
            Assert.That(result.Diagnostics.Exists(e=>e.Stage=="FALLBACK"),Is.False);
        }
        [Test, Timeout(3000)]
        public async Task CallerCancellationStillStopsChatBeforeConfiguredBudget() {
            var llm=new SlowLlm();
            using(var cts=new CancellationTokenSource(100)) {
                var result=await Service(llm,()=>900).ProcessChatAsync("xin chào, bạn có thể giúp tôi không",AiTestContexts.All(),"cancel-general",cancellationToken:cts.Token);
                Assert.AreEqual(504,result.HttpStatus);
            }
        }
        [Test, NonParallelizable]
        public void DesktopLoggerWritesOneCompleteTraceEntry() {
            string marker="unique-log-"+Guid.NewGuid().ToString("N");
            var writer=new StringWriter();var listener=new TextWriterTraceListener(writer);
            Trace.Listeners.Add(listener);
            try {
                QLyNSu.Functions.AiLogger.Info("REGRESSION",marker,"test-details");Trace.Flush();
                string output=writer.ToString();
                Assert.AreEqual(1,output.Split(new[]{marker},StringSplitOptions.None).Length-1);
                Assert.That(output,Does.Contain("test-details"));
            } finally { Trace.Listeners.Remove(listener);listener.Dispose(); }
        }
    }
}
