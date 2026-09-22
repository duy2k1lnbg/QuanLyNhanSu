using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bu.CLASS_SECURITY
{
    public interface ISessionPolicyService
    {
        Task<AuthPolicyEntity> GetEffectivePolicyAsync(decimal userId, string username, bool isAdmin);
    }

    public class SessionPolicyService : ISessionPolicyService
    {
        public async Task<AuthPolicyEntity> GetEffectivePolicyAsync(decimal userId, string username, bool isAdmin)
        {
            try
            {
                using (var db = new MyEntities())
                {
                    // 1. Kiểm tra policy riêng cấp USER
                    string userScopeId = userId.ToString();
                    var userPolicy = await db.Database.SqlQuery<AuthPolicyEntity>(@"
                        SELECT POLICY_ID, SCOPE_TYPE, SCOPE_ID, MAX_ACTIVE_SESSIONS, SESSION_LIMIT_STRATEGY,
                               ACCESS_TOKEN_MINUTES, IDLE_TIMEOUT_MINUTES, ABSOLUTE_TIMEOUT_MINUTES,
                               MAX_FAILED_LOGIN_ATTEMPTS, FAILED_ATTEMPT_WINDOW_MINUTES, LOCKOUT_DURATION_MINUTES,
                               ENABLED, CREATED_AT, UPDATED_AT
                        FROM HR.TB_AUTH_POLICY
                        WHERE SCOPE_TYPE = 'USER' AND SCOPE_ID = :p0 AND ENABLED = 1 AND ROWNUM = 1",
                        new OracleParameter("p0", userScopeId)
                    ).FirstOrDefaultAsync();

                    if (userPolicy != null) return userPolicy;

                    // 2. Kiểm tra policy cấp ROLE ('Admin' hoặc 'User')
                    string roleScopeId = isAdmin ? "Admin" : "User";
                    var rolePolicy = await db.Database.SqlQuery<AuthPolicyEntity>(@"
                        SELECT POLICY_ID, SCOPE_TYPE, SCOPE_ID, MAX_ACTIVE_SESSIONS, SESSION_LIMIT_STRATEGY,
                               ACCESS_TOKEN_MINUTES, IDLE_TIMEOUT_MINUTES, ABSOLUTE_TIMEOUT_MINUTES,
                               MAX_FAILED_LOGIN_ATTEMPTS, FAILED_ATTEMPT_WINDOW_MINUTES, LOCKOUT_DURATION_MINUTES,
                               ENABLED, CREATED_AT, UPDATED_AT
                        FROM HR.TB_AUTH_POLICY
                        WHERE SCOPE_TYPE = 'ROLE' AND LOWER(SCOPE_ID) = LOWER(:p0) AND ENABLED = 1 AND ROWNUM = 1",
                        new OracleParameter("p0", roleScopeId)
                    ).FirstOrDefaultAsync();

                    if (rolePolicy != null) return rolePolicy;

                    // 3. Kiểm tra policy cấp GLOBAL ('*')
                    var globalPolicy = await db.Database.SqlQuery<AuthPolicyEntity>(@"
                        SELECT POLICY_ID, SCOPE_TYPE, SCOPE_ID, MAX_ACTIVE_SESSIONS, SESSION_LIMIT_STRATEGY,
                               ACCESS_TOKEN_MINUTES, IDLE_TIMEOUT_MINUTES, ABSOLUTE_TIMEOUT_MINUTES,
                               MAX_FAILED_LOGIN_ATTEMPTS, FAILED_ATTEMPT_WINDOW_MINUTES, LOCKOUT_DURATION_MINUTES,
                               ENABLED, CREATED_AT, UPDATED_AT
                        FROM HR.TB_AUTH_POLICY
                        WHERE SCOPE_TYPE = 'GLOBAL' AND SCOPE_ID = '*' AND ENABLED = 1 AND ROWNUM = 1"
                    ).FirstOrDefaultAsync();

                    if (globalPolicy != null) return globalPolicy;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[SessionPolicyService] Error querying policy: " + ex.Message);
            }

            // Fallback Defaults
            return new AuthPolicyEntity
            {
                SCOPE_TYPE = isAdmin ? "ROLE" : "GLOBAL",
                SCOPE_ID = isAdmin ? "Admin" : "*",
                MAX_ACTIVE_SESSIONS = isAdmin ? 4 : 2,
                SESSION_LIMIT_STRATEGY = SessionLimitStrategies.RevokeOldest,
                ACCESS_TOKEN_MINUTES = isAdmin ? 120 : 60,
                IDLE_TIMEOUT_MINUTES = 480,
                ABSOLUTE_TIMEOUT_MINUTES = 1440,
                MAX_FAILED_LOGIN_ATTEMPTS = 5,
                FAILED_ATTEMPT_WINDOW_MINUTES = 15,
                LOCKOUT_DURATION_MINUTES = 15,
                ENABLED = 1
            };
        }
    }
}
