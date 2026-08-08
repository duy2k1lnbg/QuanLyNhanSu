using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Core;
using System;
using System.Collections.Generic;

namespace Bu.Services.AI_Services
{
    public static class AiServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        static AiServiceLocator()
        {
            // Register concrete service implementations
            var promptManager = new JsonPromptManager();
            var llmService = new OllamaService(promptManager);
            var vectorService = new Vector.QdrantService(llmService);
            var sqlGenerator = new SqlGeneratorService(llmService, promptManager);
            var router = new AiRouterService(llmService);

            _services[typeof(IPromptManager)] = promptManager;
            _services[typeof(ILlmService)] = llmService;
            _services[typeof(IVectorService)] = vectorService;
            _services[typeof(ISqlGenerator)] = sqlGenerator;
            _services[typeof(AiRouterService)] = router;
        }

        public static T GetService<T>()
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return (T)service;
            }
            throw new Exception($"Service of type {typeof(T).Name} is not registered.");
        }
    }
}
