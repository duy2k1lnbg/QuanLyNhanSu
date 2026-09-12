using System.Data;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Interfaces
{
    public class RetrievedRagContext
    {
        public string CombinedContext { get; set; } = string.Empty;
        public string SqlQuery { get; set; } = string.Empty;
        public DataTable SqlDataTable { get; set; }
        public string VectorContext { get; set; } = string.Empty;
        public string DataContext { get; set; } = string.Empty;
    }

    public interface IRagContextRetriever
    {
        Task<RetrievedRagContext> RetrieveContextAsync(string question, string intent);
    }
}
