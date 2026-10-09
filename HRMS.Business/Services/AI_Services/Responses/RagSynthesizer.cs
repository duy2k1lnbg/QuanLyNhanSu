using System;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Interfaces;

namespace Bu.Services.AI_Services.Core
{
    public class RagSynthesizer : IRagSynthesizer
    {
        private readonly ILlmService _llmService;

        public RagSynthesizer(ILlmService llmService)
        {
            _llmService = llmService;
        }

        public async Task<string> SynthesizeAnswerAsync(string context, string question, string history, Action<string> onTokenReceived = null)
        {
            return await _llmService.AskChat(context, question, history, onTokenReceived);
        }
    }
}
