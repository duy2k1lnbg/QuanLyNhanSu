using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Newtonsoft.Json;

namespace DA
{
    public partial class MyEntities
    {
        // Thuộc tính để lưu thông tin người dùng hiện tại phục vụ cho Audit Log
        public static Func<AuditIdentity> AuditIdentityProvider { get; set; }
        private static readonly System.Threading.AsyncLocal<AuditIdentity> AmbientIdentity = new System.Threading.AsyncLocal<AuditIdentity>();
        private static AuditIdentity Ambient => AmbientIdentity.Value ?? (AmbientIdentity.Value=new AuditIdentity());
        public static int? CurrentAuditUserId { get => Ambient.UserId; set { var next=Ambient.Copy(); next.UserId=value; AmbientIdentity.Value=next; } }
        public static string CurrentAuditUsername { get => Ambient.Username; set { var next=Ambient.Copy(); next.Username=value; AmbientIdentity.Value=next; } }
        public static string CurrentSessionId { get => Ambient.SessionId; set { var next=Ambient.Copy(); next.SessionId=value; AmbientIdentity.Value=next; } }
        public AuditIdentity AuditContext { get; set; } = (AuditIdentityProvider?.Invoke() ?? AmbientIdentity.Value)?.Copy();

        private string GetModuleName(string tableName)
        {
            if (string.IsNullOrEmpty(tableName)) return "Unknown";
            string upperName = tableName.ToUpper();
            if (upperName.Contains("SYS_") || upperName.Contains("CONFIG") || upperName.Contains("REPORT") || upperName.Contains("USER") || upperName.Contains("GROUP"))
                return "SYSTEM";
            if (upperName.Contains("CONG") || upperName.Contains("TANGCA") || upperName.Contains("PHUCAP") || upperName.Contains("UNGLUONG") || upperName.Contains("BANGLUONG"))
                return "PAYROLL";
            if (upperName.Contains("HOPDONG") || upperName.Contains("KHENTHUONG") || upperName.Contains("KYLUAT") || upperName.Contains("DIEUCHUYEN") || upperName.Contains("THOIVIEC") || upperName.Contains("NANGLUONG"))
                return "HR_ACTION";
            if (upperName.Contains("NHANVIEN") || upperName.Contains("PHONGBAN") || upperName.Contains("CHUCVU") || upperName.Contains("BOPHAN") || upperName.Contains("DANMUC"))
                return "HR_MASTER";
            return "GENERAL";
        }

        private void WriteAudit()
        {
            var identity=AuditContext ?? AuditIdentityProvider?.Invoke() ?? AmbientIdentity.Value ?? new AuditIdentity { Channel="SYSTEM", Username="System" };
            var modifiedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            bool? hasExtendedAuditColumns=null;
            foreach (var entry in modifiedEntities)
            {
                // Lấy tên thực thể gốc (bỏ qua Proxy class của Entity Framework)
                string tableName = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(entry.Entity.GetType()).Name;

                if (tableName == "TB_SYS_LOG" || tableName == "TB_SYS_LOGIN_HISTORY" || tableName.StartsWith("TB_AUTH_"))
                    continue;

                string action = entry.State.ToString(); // Added, Modified, Deleted

                string oldData = null;
                string newData = null;
                string recordId = "";
                string changedFieldsStr = null;

                try
                {
                    // Cố gắng lấy ID (thường cột đầu tiên là ID)
                    var keyNames = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager
                        .GetObjectStateEntry(entry.Entity).EntityKey.EntityKeyValues;
                    if (keyNames != null && keyNames.Length > 0)
                    {
                        recordId = keyNames[0].Value.ToString();
                    }
                }
                catch { }

                if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                {
                    var originalValues = entry.OriginalValues.PropertyNames
                        .Where(pn => !(entry.OriginalValues[pn] is byte[]) && !AuditIdentity.IsSensitive(pn)) // Bỏ qua dữ liệu BLOB (Hình ảnh) để tránh làm nặng Log
                        .ToDictionary(pn => pn, pn => entry.OriginalValues[pn]);
                    oldData = JsonConvert.SerializeObject(originalValues);
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var currentValues = entry.CurrentValues.PropertyNames
                        .Where(pn => !(entry.CurrentValues[pn] is byte[]) && !AuditIdentity.IsSensitive(pn)) // Bỏ qua dữ liệu BLOB (Hình ảnh)
                        .ToDictionary(pn => pn, pn => entry.CurrentValues[pn]);
                    newData = JsonConvert.SerializeObject(currentValues);
                }

                if (entry.State == EntityState.Modified)
                {
                    var diffDict = new System.Collections.Generic.Dictionary<string, object>();
                    foreach (var propName in entry.OriginalValues.PropertyNames)
                    {
                        var originalVal = entry.OriginalValues[propName];
                        var currentVal = entry.CurrentValues[propName];

                        // Bỏ qua kiểu dữ liệu BLOB để tránh lỗi Serialize
                        if (originalVal is byte[] || currentVal is byte[] || AuditIdentity.IsSensitive(propName)) continue;

                        if (!object.Equals(originalVal, currentVal))
                        {
                            diffDict[propName] = new { Old = originalVal, New = currentVal };
                        }
                    }
                    if (diffDict.Count > 0)
                    {
                        changedFieldsStr = JsonConvert.SerializeObject(diffDict);
                    }
                }

                // Ghi log bằng lệnh SQL Raw để không cần tạo Entity TB_SYS_LOG (tránh sửa EDMX rủi ro)
                try
                {
                    string currentIp = identity.IpAddress;
                    string currentMac = null;
                    string pcName = identity.DeviceName;
                    string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string moduleName = GetModuleName(tableName);

                    string sql = @"INSERT INTO HR.TB_SYS_LOG
                                  (MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI, DU_LIEU_CU, DU_LIEU_MOI, IP_ADDRESS, MAC_ADDRESS, TEN_MAY_TINH, THOIGIAN, SESSION_ID, MODULE_NAME, APP_VERSION, CHANGED_FIELDS, CLIENT_TYPE, DEVICE_ID, CORRELATION_ID, EVENT_RESULT, EXECUTOR, IP_SOURCE, USER_AGENT)
                                  VALUES (:p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, CURRENT_TIMESTAMP, :p10, :p11, :p12, :p13, :p14, :p15, :p16, 'SUCCESS', :p17, :p18, :p19)";

                    if(!hasExtendedAuditColumns.HasValue) hasExtendedAuditColumns=Database.SqlQuery<decimal>("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME='TB_SYS_LOG' AND COLUMN_NAME IN ('CLIENT_TYPE','DEVICE_ID','CORRELATION_ID','EVENT_RESULT','EXECUTOR','IP_SOURCE','USER_AGENT')").Single()==7;
                    bool extended=hasExtendedAuditColumns.Value;
                    if(!extended) sql=sql.Replace(", CLIENT_TYPE, DEVICE_ID, CORRELATION_ID, EVENT_RESULT, EXECUTOR, IP_SOURCE, USER_AGENT", "").Replace(", :p14, :p15, :p16, 'SUCCESS', :p17, :p18, :p19", "");
                    var auditParameters=new[] {
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", identity.UserId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", identity.Username ?? "System"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", action.ToUpper()),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", tableName),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p4", recordId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p5", oldData ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p6", newData ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p7", currentIp ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p8", currentMac ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p9", pcName ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p10", identity.SessionId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p11", moduleName ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p12", appVersion ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p13", changedFieldsStr ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p14", identity.Channel ?? "UNKNOWN"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p15", identity.DeviceId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p16", identity.CorrelationId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p17", identity.Executor ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p18", identity.IpSource ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p19", identity.UserAgent ?? (object)DBNull.Value)};
                    Database.ExecuteSqlCommand(sql, auditParameters.Take(extended?20:14).Cast<object>().ToArray());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Audit write failed: " + ex.GetType().Name);
                    throw; // A successful change and its audit must commit together.
                }
            }

        }
        public override int SaveChanges()
        {
            using(var ownTransaction=Database.CurrentTransaction==null ? Database.BeginTransaction() : null) {
                WriteAudit(); var changed=base.SaveChanges(); ownTransaction?.Commit(); return changed;
            }
        }
        public override async System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using(var ownTransaction=Database.CurrentTransaction==null ? Database.BeginTransaction() : null) {
                WriteAudit(); var changed=await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false); ownTransaction?.Commit();return changed;
            }
        }
        public override System.Threading.Tasks.Task<int> SaveChangesAsync() => SaveChangesAsync(System.Threading.CancellationToken.None);
    }
    public sealed class AuditIdentity
    {
        public int? UserId; public string Username,SessionId,Channel,DeviceId,DeviceName,IpAddress,IpSource,UserAgent,Executor,CorrelationId;
        public AuditIdentity Copy() => (AuditIdentity)MemberwiseClone();
        public static bool IsSensitive(string name)
        {
            string n=(name??"").ToUpperInvariant();
            return new[]{"PASSWORD","MATKHAU","TOKEN","SECRET","JTI","CCCD","CMND","MA_SO_THUE","SO_HO_CHIEU","SESSION_KEY"}.Any(n.Contains);
        }
        public static string RedactJson(string input)
        {
            if(string.IsNullOrEmpty(input)) return input;
            try { var token=Newtonsoft.Json.Linq.JToken.Parse(input); Redact(token);return JsonConvert.SerializeObject(token,Newtonsoft.Json.Formatting.None); }
            catch { return input; }
        }
        private static void Redact(Newtonsoft.Json.Linq.JToken token)
        {
            foreach(var child in token.Children().ToList()) {
                var property=child as Newtonsoft.Json.Linq.JProperty;
                if(property!=null && IsSensitive(property.Name)) property.Value="[REDACTED]";
                else Redact(child);
            }
        }
    }

}
