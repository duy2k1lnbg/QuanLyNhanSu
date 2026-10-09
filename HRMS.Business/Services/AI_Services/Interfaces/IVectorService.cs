using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Vector;

namespace Bu.Services.AI_Services.Interfaces
{
    public interface IVectorService
    {
        void Add(string text, string tag = "GENERAL");
        void Add(string text, string tag, int? employeeId);
        List<string> Search(string query, string tag = null);
        Task<VectorSearchResult> SearchScopedAsync(
            string query,
            VectorBusinessFilter businessFilter,
            VectorSecurityFilter securityFilter,
            float[] precomputedEmbedding = null,
            CancellationToken cancellationToken = default);
        void Clear();
        void RemoveByEmployeeId(int manv);
        void SyncEmployeeData(int manv);
    }
}
