using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bu.Services.AI_Services.Interfaces;
using Bu.Services.AI_Services.Security;
using DA;
using Oracle.ManagedDataAccess.Client;

namespace Bu.Services.AI_Services.Core
{
    public class ScopedSqlExecutor : IScopedSqlExecutor, IAuthorizedSqlExecutor, ISafeSqlExecutor
    {
        private readonly IAiHmacProofService _proofService;

        public ScopedSqlExecutor(IAiHmacProofService proofService = null)
        {
            _proofService = proofService ?? new AiHmacProofService();
        }

        public SqlExecutionResult ExecuteScopedQuery(string sql, Dictionary<string,object> parameters = null, CancellationToken cancellationToken = default)
        {
            var valid = OracleSqlAstValidator.Validate(sql);
            return new SqlExecutionResult { Status = valid.IsValid ? SqlExecutionStatus.AuthorizationDenied : SqlExecutionStatus.ValidationRejected, ErrorMessage = "Cần kế hoạch và ngữ cảnh phân quyền đã xác thực." };
        }
        public Task<SqlExecutionResult> ExecuteScopedQueryAsync(string sql, Dictionary<string,object> parameters = null, CancellationToken cancellationToken = default) => Task.FromResult(ExecuteScopedQuery(sql,parameters,cancellationToken));
        public DataTable ExecuteSafeQuery(string sql) => throw new InvalidOperationException("Không được thực thi SQL AI thiếu ngữ cảnh phân quyền.");
        public Task<SqlExecutionResult> ExecutePlanAsync(QueryExecutionPlan plan, AiAuthorizationContext ctx, CancellationToken cancellationToken = default)
        {
            bool isSupportedStrategy = plan?.Strategy == ExecutionStrategy.SqlTemplate || plan?.Strategy == ExecutionStrategy.Hybrid;
            if (ctx == null || !ctx.PolicyLoaded || !isSupportedStrategy || ctx.Fingerprint() != plan.AuthorizationFingerprint || !AiAuthorizationService.ValidateCapability(ctx,plan.RequiredCapability).IsAllowed)
                return Task.FromResult(new SqlExecutionResult { Status=SqlExecutionStatus.AuthorizationDenied });
            var validation = OracleSqlAstValidator.Validate(plan.SqlStatement);
            if (!validation.IsValid || validation.ReferencedTables.Count != 1 || !string.Equals(validation.ReferencedTables[0],plan.TargetView,StringComparison.OrdinalIgnoreCase) || AiCapabilityCatalog.Get(plan.RequiredCapability)?.SourceView != plan.TargetView)
                return Task.FromResult(new SqlExecutionResult { Status=SqlExecutionStatus.ValidationRejected });
            return Task.Run(() => ExecuteAuthorizedQuery(plan.SqlStatement,plan.Parameters,ctx,plan.RequiredCapability,plan.RowLimit,plan.IsScalar,cancellationToken,plan.RequestedScope == "SELF"),cancellationToken);
        }
        internal SqlExecutionResult ExecuteAuthorizedQuery(string sql, Dictionary<string,object> parameters, AiAuthorizationContext ctx, string capability, int rowLimit = 20, bool scalar = false, CancellationToken cancellationToken = default, bool selfOnly = false)
        {
            var validation = OracleSqlAstValidator.Validate(sql);
            if (!validation.IsValid || ctx == null || !ctx.PolicyLoaded || !AiAuthorizationService.ValidateCapability(ctx,capability).IsAllowed) return new SqlExecutionResult { Status=SqlExecutionStatus.AuthorizationDenied };
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                string ticket = null;
                // 100% AI_READONLY connection: Issue and consume ticket in the same session without using HR
                using (var db = new AiEntities())
                {
                    var connection = (OracleConnection)db.Database.Connection;
                    connection.OpenAsync(cancellationToken).GetAwaiter().GetResult();
                    try
                    {
                        var proof = _proofService.GenerateProof(ctx.UserId, capability);
                        using (var issue = connection.CreateCommand())
                        {
                            issue.BindByName = true;
                            issue.CommandTimeout = 5;
                            issue.CommandType = CommandType.StoredProcedure;
                            issue.CommandText = "AI_OWNER.PKG_AI_AUTH.VERIFY_AND_ISSUE_TICKET";
                            issue.Parameters.Add(CreateOracleParameter("p_actor_id", ctx.UserId));
                            issue.Parameters.Add(CreateOracleParameter("p_capability", capability));
                            issue.Parameters.Add(CreateOracleParameter("p_audience", proof.Audience));
                            issue.Parameters.Add(CreateOracleParameter("p_nonce", proof.Nonce));
                            issue.Parameters.Add(CreateOracleParameter("p_exp", proof.ExpEpochSeconds));
                            issue.Parameters.Add(CreateOracleParameter("p_signature", proof.SignatureHex));
                            var output = new OracleParameter("p_ticket", OracleDbType.Varchar2, 64) { Direction = ParameterDirection.Output };
                            issue.Parameters.Add(output);
                            using (cancellationToken.Register(() => { try { issue.Cancel(); } catch { } })) issue.ExecuteNonQuery();
                            ticket = output.Value?.ToString();
                        }

                        using (var bind = connection.CreateCommand())
                        {
                            bind.BindByName = true;
                            bind.CommandTimeout = 5;
                            bind.CommandText = "BEGIN AI_OWNER.PKG_AI_READER.BIND_TICKET(:p_ticket, :p_self); END;";
                            bind.Parameters.Add(CreateOracleParameter("p_ticket", ticket));
                            bind.Parameters.Add(CreateOracleParameter("p_self", selfOnly ? 1 : 0));
                            bind.ExecuteNonQuery();
                        }
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandTimeout = 15;
                            PrepareCommand(cmd, validation.CleanedSql, parameters);
                            using (cancellationToken.Register(() => { try { cmd.Cancel(); } catch { } }))
                            using (var reader = cmd.ExecuteReader())
                            {
                                var table = new DataTable();
                                for (int i = 0; i < reader.FieldCount; i++) table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                                while (table.Rows.Count < rowLimit + 1 && reader.Read())
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    var values = new object[reader.FieldCount];
                                    reader.GetValues(values);
                                    table.Rows.Add(values);
                                }
                                bool more = !scalar && table.Rows.Count > rowLimit;
                                int? total = scalar ? (int?)1 : null;
                                if (table.Columns.Contains("AI_TOTAL_COUNT"))
                                {
                                    if (table.Rows.Count > 0) total = Convert.ToInt32(table.Rows[0]["AI_TOTAL_COUNT"]);
                                    else total = 0;
                                    table.Columns.Remove("AI_TOTAL_COUNT");
                                }
                                if (more) table.Rows.RemoveAt(rowLimit);
                                cancellationToken.ThrowIfCancellationRequested();
                                return new SqlExecutionResult
                                {
                                    Status = table.Rows.Count > 0 ? SqlExecutionStatus.SuccessWithData : SqlExecutionStatus.SuccessEmpty,
                                    Data = table,
                                    TotalRecords = total,
                                    HasMore = more,
                                    SourceProvenance = validation.ReferencedTables[0]
                                };
                            }
                        }
                    }
                    finally
                    {
                        // Cleanup runs even after cancellation; never return a poisoned context to the pool.
                        try
                        {
                            using (var clear = connection.CreateCommand())
                            {
                                clear.CommandTimeout = 5;
                                clear.CommandText = "BEGIN AI_OWNER.PKG_AI_READER.CLEAR_REQUEST; END;";
                                clear.ExecuteNonQuery();
                            }
                        }
                        catch
                        {
                            OracleConnection.ClearPool(connection);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { return new SqlExecutionResult { Status=SqlExecutionStatus.Timeout }; }
            catch (OracleException ex)
            {
                System.Diagnostics.Trace.TraceError("AI Oracle error code {0}",ex.Number);
                return new SqlExecutionResult { Status=ex.Number==1013 ? SqlExecutionStatus.Timeout : new[] {20001,20002,20003}.Contains(ex.Number) ? SqlExecutionStatus.AuthorizationDenied : new[] {942,904,6550,4043}.Contains(ex.Number) ? SqlExecutionStatus.SourceUnavailable : SqlExecutionStatus.ExecutionError, ErrorMessage = "ORA-" + ex.Number + ": " + ex.Message };
            }
            catch (Exception ex) { return new SqlExecutionResult { Status=SqlExecutionStatus.ConnectionError, ErrorMessage = ex.GetType().Name + ": " + ex.Message }; }
        }
        public static void PrepareCommand(IDbCommand cmd, string sql, Dictionary<string, object> parameters)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));
            cmd.CommandText = sql;

            // R04 fix: Set BindByName = true for OracleCommand to prevent positional binding errors
            if (cmd is OracleCommand ocmd)
            {
                ocmd.BindByName = true;
            }
            else
            {
                var prop = cmd.GetType().GetProperty("BindByName");
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(cmd, true, null);
                }
            }

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    var oracleParam = CreateOracleParameter(p.Key, p.Value);
                    cmd.Parameters.Add(oracleParam);
                }
            }
        }

        public static OracleParameter CreateOracleParameter(string name, object value)
        {
            string paramName = name;
            if (!string.IsNullOrEmpty(paramName) && paramName.StartsWith(":"))
            {
                paramName = paramName.Substring(1);
            }

            if (value == null || value == DBNull.Value)
            {
                return new OracleParameter(paramName, OracleDbType.Varchar2) { Value = DBNull.Value };
            }

            if (value is int intVal)
            {
                return new OracleParameter(paramName, OracleDbType.Int32) { Value = intVal };
            }
            if (value is long longVal)
            {
                return new OracleParameter(paramName, OracleDbType.Int64) { Value = longVal };
            }
            if (value is short shortVal)
            {
                return new OracleParameter(paramName, OracleDbType.Int16) { Value = shortVal };
            }
            if (value is decimal decVal)
            {
                return new OracleParameter(paramName, OracleDbType.Decimal) { Value = decVal };
            }
            if (value is double dblVal)
            {
                return new OracleParameter(paramName, OracleDbType.Decimal) { Value = Convert.ToDecimal(dblVal) };
            }
            if (value is float fltVal)
            {
                return new OracleParameter(paramName, OracleDbType.Decimal) { Value = Convert.ToDecimal(fltVal) };
            }
            if (value is DateTime dtVal)
            {
                return new OracleParameter(paramName, OracleDbType.Date) { Value = dtVal };
            }
            if (value is bool bVal)
            {
                return new OracleParameter(paramName, OracleDbType.Int32) { Value = bVal ? 1 : 0 };
            }

            return new OracleParameter(paramName, OracleDbType.NVarchar2) { Value = value.ToString() };
        }
    }
}
