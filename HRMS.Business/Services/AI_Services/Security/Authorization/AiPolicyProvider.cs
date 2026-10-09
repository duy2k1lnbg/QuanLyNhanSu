using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Bu.Services.AI_Services.Core;
using DA;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace Bu.Services.AI_Services.Security
{
    public interface IAiPolicyProvider { AiAuthorizationContext Load(int userId); }
    public interface IAiIdentityProvider { AiAuthorizationContext LoadIdentity(int userId); }
    public interface IAiReadinessProbe { bool IsQueryReady(); }

    public class AiSourceUnavailableException : Exception
    {
        public string ErrorCode { get; }
        public string Stage { get; }
        public AiSourceUnavailableException() : this("AI_SOURCE_UNAVAILABLE", "unknown", null) { }
        public AiSourceUnavailableException(string code, string stage, Exception cause)
            : base("Nguồn dữ liệu hoặc chính sách AI chưa sẵn sàng.", cause)
        {
            ErrorCode = code == "AI_SETUP_REQUIRED" ? code : "AI_SOURCE_UNAVAILABLE";
            Stage = stage;
        }
    }

    public sealed class OracleAiPolicyProvider : IAiPolicyProvider, IAiIdentityProvider
    {
        private readonly IAiHmacProofService _proofService;

        public OracleAiPolicyProvider(IAiHmacProofService proofService = null)
        {
            _proofService = proofService ?? new AiHmacProofService();
        }

        public AiAuthorizationContext Load(int userId)
        {
            string stage = "identity";
            try
            {
                // 100% AI_READONLY connection: reads snapshot authenticated via PKG_AI_AUTH with HMAC Proof
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var proof = _proofService.GenerateProof(userId, "ACTOR_SNAPSHOT");
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_AUTH.GET_ACTOR_SNAPSHOT";
                        cmd.Parameters.Add(new OracleParameter("p_actor_id", OracleDbType.Int32) { Value = userId });
                        cmd.Parameters.Add(new OracleParameter("p_audience", OracleDbType.Varchar2) { Value = proof.Audience });
                        cmd.Parameters.Add(new OracleParameter("p_nonce", OracleDbType.Varchar2) { Value = proof.Nonce });
                        cmd.Parameters.Add(new OracleParameter("p_exp", OracleDbType.Int64) { Value = proof.ExpEpochSeconds });
                        cmd.Parameters.Add(new OracleParameter("p_signature", OracleDbType.Varchar2) { Value = proof.SignatureHex });

                        var pUser = new OracleParameter("p_user_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        var pRights = new OracleParameter("p_rights_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        var pCaps = new OracleParameter("p_caps_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        var pFields = new OracleParameter("p_fields_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        var pGrants = new OracleParameter("p_grants_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        var pRevs = new OracleParameter("p_revs_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };

                        cmd.Parameters.Add(pUser);
                        cmd.Parameters.Add(pRights);
                        cmd.Parameters.Add(pCaps);
                        cmd.Parameters.Add(pFields);
                        cmd.Parameters.Add(pGrants);
                        cmd.Parameters.Add(pRevs);

                        cmd.ExecuteNonQuery();

                        // 1. Read User Identity
                        var ctx = new AiAuthorizationContext();
                        using (var reader = ((OracleRefCursor)pUser.Value).GetDataReader())
                        {
                            if (!reader.Read()) return AiAuthorizationContext.CreateAnonymous();
                            ctx.UserId = userId;
                            ctx.Username = reader["USERNAME"]?.ToString();
                            ctx.FullName = reader["FULLNAME"]?.ToString();
                            ctx.Manv = reader["MANV"] != DBNull.Value ? (int?)Convert.ToInt32(reader["MANV"]) : null;
                            ctx.MaCty = reader["MACTY"]?.ToString();
                            ctx.PolicyVersion = Convert.ToInt64(reader["POLICY_VERSION"]);
                            ctx.PolicyLoaded = true;
                        }

                        // 2. Read Function Rights
                        stage = "rights";
                        using (var reader = ((OracleRefCursor)pRights.Value).GetDataReader())
                        {
                            var rights = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            while (reader.Read())
                            {
                                rights.Add(reader["FUNCTION_CODE"].ToString());
                            }
                            ctx.FunctionRights = rights;
                        }

                        // 3. Read Capabilities
                        stage = "capabilities";
                        using (var reader = ((OracleRefCursor)pCaps.Value).GetDataReader())
                        {
                            while (reader.Read())
                            {
                                var capCode = reader["CAPABILITY_CODE"].ToString();
                                ctx.Capabilities[capCode] = new AiPolicyCapability
                                {
                                    CapabilityCode = capCode,
                                    RequiredFunctionCode = reader["REQUIRED_FUNCTION_CODE"]?.ToString(),
                                    SourceView = reader["SOURCE_VIEW"]?.ToString(),
                                    Enabled = Convert.ToInt32(reader["IS_ENABLED"]) == 1
                                };
                            }
                        }

                        // 4. Read Field Policies
                        stage = "field_policies";
                        using (var reader = ((OracleRefCursor)pFields.Value).GetDataReader())
                        {
                            while (reader.Read())
                            {
                                var capCode = reader["CAPABILITY_CODE"].ToString();
                                var field = reader["LOGICAL_FIELD"].ToString();
                                var key = capCode + ":" + field;
                                if (ctx.FieldPolicies.ContainsKey(key)) throw new AiSourceUnavailableException();
                                ctx.FieldPolicies[key] = reader["ACCESS_MODE"]?.ToString();
                                ctx.FieldOperations[key] = reader["ALLOWED_OPERATIONS"]?.ToString();
                            }
                        }

                        // 5. Read Scope Grants
                        stage = "scope_grants";
                        using (var reader = ((OracleRefCursor)pGrants.Value).GetDataReader())
                        {
                            while (reader.Read())
                            {
                                ctx.ScopeGrants.Add(new AiScopeGrant
                                {
                                    CapabilityCode = reader["CAPABILITY_CODE"].ToString(),
                                    ScopeType = reader["SCOPE_TYPE"].ToString(),
                                    ScopeKey = reader["SCOPE_KEY"]?.ToString(),
                                    Effect = reader["EFFECT"].ToString(),
                                    ValidTo = reader["VALID_TO"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["VALID_TO"]) : null
                                });
                            }
                        }

                        // 6. Read Revisions
                        stage = "revisions";
                        using (var reader = ((OracleRefCursor)pRevs.Value).GetDataReader())
                        {
                            while (reader.Read())
                            {
                                string revKey = reader["REVISION_KEY"].ToString();
                                long revNum = Convert.ToInt64(reader["REVISION_NUMBER"]);
                                if (revKey.StartsWith("DATA_", StringComparison.Ordinal))
                                {
                                    ctx.SourceRevisions[revKey.Substring(5)] = revNum;
                                }
                            }
                        }

                        return ctx;
                    }
                }
            }
            catch (AiSourceUnavailableException) { throw; }
            catch (Exception ex) { throw SourceError(stage, ex); }
        }

        public AiAuthorizationContext LoadIdentity(int userId)
        {
            string stage = "identity";
            try
            {
                using (var db = new AiEntities())
                {
                    var conn = (OracleConnection)db.Database.Connection;
                    if (conn.State != ConnectionState.Open) conn.Open();

                    var proof = _proofService.GenerateProof(userId, "ACTOR_IDENTITY");
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.BindByName = true;
                        cmd.CommandTimeout = 10;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "AI_OWNER.PKG_AI_AUTH.GET_ACTOR_IDENTITY";
                        cmd.Parameters.Add(new OracleParameter("p_actor_id", OracleDbType.Int32) { Value = userId });
                        cmd.Parameters.Add(new OracleParameter("p_audience", OracleDbType.Varchar2) { Value = proof.Audience });
                        cmd.Parameters.Add(new OracleParameter("p_nonce", OracleDbType.Varchar2) { Value = proof.Nonce });
                        cmd.Parameters.Add(new OracleParameter("p_exp", OracleDbType.Int64) { Value = proof.ExpEpochSeconds });
                        cmd.Parameters.Add(new OracleParameter("p_signature", OracleDbType.Varchar2) { Value = proof.SignatureHex });

                        var pUser = new OracleParameter("p_user_cur", OracleDbType.RefCursor) { Direction = ParameterDirection.Output };
                        var pHasAi = new OracleParameter("p_has_ai_right", OracleDbType.Int32) { Direction = ParameterDirection.Output };

                        cmd.Parameters.Add(pUser);
                        cmd.Parameters.Add(pHasAi);

                        cmd.ExecuteNonQuery();

                        int hasAi = pHasAi.Value != DBNull.Value ? Convert.ToInt32(pHasAi.Value.ToString()) : 0;
                        if (hasAi == 0) return AiAuthorizationContext.CreateAnonymous();

                        var ctx = new AiAuthorizationContext();
                        using (var reader = ((OracleRefCursor)pUser.Value).GetDataReader())
                        {
                            if (!reader.Read()) return AiAuthorizationContext.CreateAnonymous();
                            ctx.UserId = userId;
                            ctx.Username = reader["USERNAME"]?.ToString();
                            ctx.FullName = reader["FULLNAME"]?.ToString();
                            ctx.Manv = reader["MANV"] != DBNull.Value ? (int?)Convert.ToInt32(reader["MANV"]) : null;
                            ctx.MaCty = reader["MACTY"]?.ToString();
                            ctx.PolicyLoaded = false;
                            ctx.FunctionRights.Add("F_SYSTEM_AI");
                        }
                        return ctx;
                    }
                }
            }
            catch (AiSourceUnavailableException) { throw; }
            catch (Exception ex) { throw SourceError(stage, ex); }
        }

        private static AiSourceUnavailableException SourceError(string stage, Exception exception)
        {
            OracleException oracle = null;
            for (var cause = exception; cause != null; cause = cause.InnerException)
            {
                if (cause is OracleException found) { oracle = found; break; }
            }
            bool missing = oracle != null && new[] { 942, 904, 4043, 6550, 20001, 20002, 20004 }.Contains(oracle.Number);
            System.Diagnostics.Trace.TraceError("AI policy load failed: stage={0}, oracleCode={1}, exceptionType={2}", stage, oracle?.Number, exception.GetType().Name);
            return new AiSourceUnavailableException(missing ? "AI_SETUP_REQUIRED" : "AI_SOURCE_UNAVAILABLE", stage, exception);
        }
    }

    public sealed class OracleAiReadinessProbe : IAiReadinessProbe
    {
        private static readonly object Gate = new object();
        private static DateTime validUntil;
        private static bool ready;

        public bool IsQueryReady()
        {
            lock (Gate)
            {
                if (DateTime.UtcNow < validUntil) return ready;
                ready = ReadSchemaReadiness();
                validUntil = DateTime.UtcNow.AddSeconds(10);
                return ready;
            }
        }

        private static bool ReadSchemaReadiness()
        {
            try
            {
                // 100% AI_READONLY connection checking AI_OWNER schema readiness via definer package
                using (var db = new AiEntities())
                {
                    var builder = new OracleConnectionStringBuilder(db.Database.Connection.ConnectionString) { ConnectionTimeout = 3 };
                    using (var connection = new OracleConnection(builder.ConnectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandTimeout = 3;
                            command.CommandText = "SELECT AI_OWNER.PKG_AI_READER.IS_READY() FROM DUAL";
                            return Convert.ToInt32(command.ExecuteScalar()) == 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AI readiness probe failed: {0}", ex.GetType().Name);
                return false;
            }
        }
    }
}