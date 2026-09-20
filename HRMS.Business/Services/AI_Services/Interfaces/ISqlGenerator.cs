using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Interfaces
{
    public interface ISqlGenerator
    {
        Task<string> GenerateRawSql(string question);
    }
}
