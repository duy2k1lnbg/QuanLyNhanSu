using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Bu.Services.AI_Services.Interfaces
{
    public enum SqlExecutionStatus
    {
        SuccessWithData,
        SuccessEmpty,
        ValidationRejected,
        AuthorizationDenied,
        Timeout,
        ConnectionError,
        SourceUnavailable,
        ExecutionError
    }

    public class SqlExecutionResult
    {
        public SqlExecutionStatus Status { get; set; }
        public DataTable Data { get; set; }
        public int? TotalRecords { get; set; }
        public long ExecutionTimeMs { get; set; }
        public string ErrorMessage { get; set; }
        public string SourceProvenance { get; set; }
        public bool HasMore { get; set; }
    }

    public interface IAuthorizedSqlExecutor
    {
        Task<SqlExecutionResult> ExecutePlanAsync(Bu.Services.AI_Services.Core.QueryExecutionPlan plan, Bu.Services.AI_Services.Security.AiAuthorizationContext ctx, CancellationToken cancellationToken = default);
    }
    public interface IScopedSqlExecutor
    {
        SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default);
        Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string, object> parameters = null, CancellationToken cancellationToken = default);
    }
}
