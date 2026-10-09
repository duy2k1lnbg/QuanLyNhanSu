using System;
using System.Collections.Concurrent;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Core;
namespace Bu.Services.AI_Services
{
    public static class AiServiceLocator
    {
        private static readonly ConcurrentDictionary<Type,Lazy<object>> Services = new ConcurrentDictionary<Type,Lazy<object>>();
        public static T GetService<T>() => (T)Services.GetOrAdd(typeof(T),t => new Lazy<object>(() => Create(t))).Value;
        private static object Create(Type type)
        {
            if (type==typeof(AiExecutionService)) return new AiExecutionService();
            if (type==typeof(Security.IAiPolicyProvider)) return new Security.OracleAiPolicyProvider();
            if (type==typeof(IClockProvider)) return new SystemClockProvider();
            if (type==typeof(IQueryUnderstandingService)) return new QueryUnderstandingService();
            if (type==typeof(IQueryPlanner)) return new QueryPlanner();
            if (type==typeof(IScopedSqlExecutor)) return new ScopedSqlExecutor();
            if (type==typeof(Security.AiAuthorizationService)) return new Security.AiAuthorizationService();
            if (type==typeof(IPromptManager)) return new JsonPromptManager();
            if (type==typeof(ILlmService)) return new OllamaService(GetService<IPromptManager>());
            if (type==typeof(IVectorService)) return new Vector.QdrantService(GetService<ILlmService>());
            if (type==typeof(ISqlGenerator)) return new SqlGeneratorService(GetService<ILlmService>(),GetService<IPromptManager>());
            if (type==typeof(AiRouterService)) return new AiRouterService(GetService<ILlmService>());
            if (type==typeof(ISafeSqlExecutor)) return new SafeSqlExecutor();
            if (type==typeof(IRagContextRetriever)) return new RagContextRetriever(GetService<ISqlGenerator>(),GetService<ISafeSqlExecutor>(),GetService<IVectorService>());
            if (type==typeof(IRagSynthesizer)) return new RagSynthesizer(GetService<ILlmService>());
            throw new InvalidOperationException("AI service is not registered: " + type.Name);
        }
    }
}