using System;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Interfaces
{
    public interface IRagSynthesizer
    {
        Task<string> SynthesizeAnswerAsync(string context, string question, string history, Action<string> onTokenReceived = null);
    }
}
