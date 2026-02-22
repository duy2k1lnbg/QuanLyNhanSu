using DA;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;

public class AIQueryService
{
    public bool IsSafeSelect(string sql)
    {
        sql = sql.ToUpper();
        if (!sql.Trim().StartsWith("SELECT"))
            return false;

        string[] blocked = { "INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "TRUNCATE", "MERGE" };
        return !blocked.Any(b => sql.Contains(b));
    }

    public DataTable Execute(string sql)
    {
        using (var ctx = new AIEntities())
        {
            var result = ctx.Database.SqlQuery<object>(sql).ToList();

            // Convert list sang DataTable
            var dt = new DataTable();
            if (result.Count > 0)
            {
                // Thêm cột từ property của object
                foreach (var prop in result[0].GetType().GetProperties())
                    dt.Columns.Add(prop.Name, prop.PropertyType);

                // Thêm dữ liệu
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