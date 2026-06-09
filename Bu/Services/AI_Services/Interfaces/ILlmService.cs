using System;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Interfaces
{
    public interface ILlmService
    {
        Task<string> AskSql(string prompt);
        Task<string> AskChat(string context, string question, string history, Action<string> onTokenReceived = null);
        Task<float[]> GetEmbedding(string text, System.Threading.CancellationToken cancellationToken = default);
    }
}
