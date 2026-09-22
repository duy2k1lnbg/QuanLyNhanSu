using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Threading.Tasks;

namespace Bu.CLASS_SECURITY
{
    public interface IAuthAuditService
    {
        Task LogEventAsync(
            decimal? userId,
            decimal? actorUserId,
            string sessionId,
            string eventType,
            string result,
            string reason,
            string clientType,
            string deviceIdHash,
            string ipAddress,
            string userAgent,
            string correlationId,
            object metadata = null
        );

        Task RecordLoginAttemptAsync(
            decimal? userId,
            string loginIdentifier,
            string clientType,
            string deviceIdHash,
            string ipAddress,
            string userAgent,
            bool isSuccess,
            string failureReason,
            string correlationId
        );
    }

    public class AuthAuditService : IAuthAuditService
    {
        public async Task LogEventAsync(
            decimal? userId,
            decimal? actorUserId,
            string sessionId,
            string eventType,
            string result,
            string reason,
            string clientType,
            string deviceIdHash,
            string ipAddress,
            string userAgent,
            string correlationId,
            object metadata = null)
        {
            try
            {
                string metadataJson = null;
                if (metadata != null)
                {
                    try
                    {
                        metadataJson = JsonConvert.SerializeObject(metadata);
                    }
                    catch { }
                }

                using (var db = new MyEntities())
                {
                    const string sql = @"
                        INSERT INTO HR.TB_AUTH_AUDIT (
                            USER_ID, ACTOR_USER_ID, SESSION_ID, EVENT_TYPE, RESULT, REASON,
                            CLIENT_TYPE, DEVICE_ID_HASH, IP_ADDRESS, USER_AGENT, OCCURRED_AT, CORRELATION_ID, METADATA_JSON
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, CURRENT_TIMESTAMP, :p10, :p11
                        )";

                    await db.Database.ExecuteSqlCommandAsync(sql,
                        userId.HasValue ? (object)new OracleParameter("p0", userId.Value) : new OracleParameter("p0", DBNull.Value),
                        actorUserId.HasValue ? (object)new OracleParameter("p1", actorUserId.Value) : new OracleParameter("p1", DBNull.Value),
                        new OracleParameter("p2", (object)sessionId ?? DBNull.Value),
                        new OracleParameter("p3", eventType ?? "UNKNOWN"),
                        new OracleParameter("p4", result ?? "SUCCESS"),
                        new OracleParameter("p5", (object)reason ?? DBNull.Value),
                        new OracleParameter("p6", (object)clientType ?? DBNull.Value),
                        new OracleParameter("p7", (object)deviceIdHash ?? DBNull.Value),
                        new OracleParameter("p8", (object)ipAddress ?? DBNull.Value),
                        new OracleParameter("p9", (object)userAgent ?? DBNull.Value),
                        new OracleParameter("p10", (object)correlationId ?? DBNull.Value),
                        new OracleParameter("p11", (object)metadataJson ?? DBNull.Value)
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AuthAuditService] LogEventAsync error: " + ex.Message);
            }
        }

        public async Task RecordLoginAttemptAsync(
            decimal? userId,
            string loginIdentifier,
            string clientType,
            string deviceIdHash,
            string ipAddress,
            string userAgent,
            bool isSuccess,
            string failureReason,
            string correlationId)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    const string sql = @"
                        INSERT INTO HR.TB_AUTH_LOGIN_ATTEMPT (
                            USER_ID, LOGIN_IDENTIFIER, CLIENT_TYPE, DEVICE_ID_HASH, IP_ADDRESS,
                            USER_AGENT, ATTEMPTED_AT, SUCCESS_FLAG, FAILURE_REASON, CORRELATION_ID
                        ) VALUES (
                            :p0, :p1, :p2, :p3, :p4, :p5, CURRENT_TIMESTAMP, :p6, :p7, :p8
                        )";

                    await db.Database.ExecuteSqlCommandAsync(sql,
                        userId.HasValue ? (object)new OracleParameter("p0", userId.Value) : new OracleParameter("p0", DBNull.Value),
                        new OracleParameter("p1", (object)loginIdentifier ?? "UNKNOWN"),
                        new OracleParameter("p2", (object)clientType ?? DBNull.Value),
                        new OracleParameter("p3", (object)deviceIdHash ?? DBNull.Value),
                        new OracleParameter("p4", (object)ipAddress ?? DBNull.Value),
                        new OracleParameter("p5", (object)userAgent ?? DBNull.Value),
                        new OracleParameter("p6", isSuccess ? 1 : 0),
                        new OracleParameter("p7", (object)failureReason ?? DBNull.Value),
                        new OracleParameter("p8", (object)correlationId ?? DBNull.Value)
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[AuthAuditService] RecordLoginAttemptAsync error: " + ex.Message);
            }
        }
    }
}
