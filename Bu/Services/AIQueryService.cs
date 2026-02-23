using DA;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;

namespace Bu.Services
{
    public class AIQueryService
    {
        public bool IsSafeSelect(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return false;

            sql = sql.ToUpper().TrimStart();
            if (!sql.StartsWith("SELECT"))
                return false;

            string[] blocked = { "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "MERGE" };
            return !blocked.Any(b => sql.Contains(b));
        }

        public DataTable Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return new DataTable();

            using (var ctx = new AIEntities())
            {
                var result = ctx.Database.SqlQuery<object>(sql).ToList();

                var dt = new DataTable();
                if (result.Count > 0)
                {
                    foreach (var prop in result[0].GetType().GetProperties())
                        dt.Columns.Add(prop.Name, prop.PropertyType);

                    foreach (var item in result)
                    {
                        var row = dt.NewRow();
                        foreach (var prop in item.GetType().GetProperties())
                            row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                        dt.Rows.Add(row);
                    }
                }
                return dt;
            }
        }
    }
}