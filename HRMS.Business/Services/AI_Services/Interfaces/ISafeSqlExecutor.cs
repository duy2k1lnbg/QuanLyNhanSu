using System.Data;

namespace Bu.Services.AI_Services.Interfaces
{
    public interface ISafeSqlExecutor
    {
        DataTable ExecuteSafeQuery(string sql);
    }
}
