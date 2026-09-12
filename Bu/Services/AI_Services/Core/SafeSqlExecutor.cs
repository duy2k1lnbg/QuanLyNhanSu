using System;
using System.Data;
using Bu.Services.AI_Services.Interfaces;
using DA;

namespace Bu.Services.AI_Services.Core
{
    public class SafeSqlExecutor : ISafeSqlExecutor
    {
        private const int CommandTimeoutSeconds = 15;

        public DataTable ExecuteSafeQuery(string sql)
        {
            var dt = new DataTable();
            if (string.IsNullOrWhiteSpace(sql))
            {
                return dt;
            }

            // 1. Kiểm tra an toàn qua AST Validator lần cuối trước khi xuống DB
            var validation = OracleSqlAstValidator.Validate(sql);
            if (!validation.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"[SAFE SQL EXECUTOR BLOCKED]: {validation.RejectionReason} (Query: {sql})");
                return dt;
            }

            string cleanSql = validation.CleanedSql;

            try
            {
                using (var db = new AiEntities())
                {
                    var conn = db.Database.Connection;
                    if (conn.State != ConnectionState.Open)
                    {
                        conn.Open();
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = cleanSql;
                        cmd.CommandTimeout = CommandTimeoutSeconds;

                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SAFE SQL EXECUTOR ERROR]: {ex.Message} (Query: {cleanSql})");
            }

            return dt;
        }
    }
}
