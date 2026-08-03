using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Newtonsoft.Json;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace DA
{
    public partial class MyEntities
    {
        // Thuộc tính để lưu thông tin người dùng hiện tại phục vụ cho Audit Log
        public static int? CurrentAuditUserId { get; set; }
        public static string CurrentAuditUsername { get; set; }
        public static string CurrentSessionId { get; set; }

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

        private string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        private string GetMacAddress()
        {
            try
            {
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        return nic.GetPhysicalAddress().ToString();
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        public override int SaveChanges()
        {
            var modifiedEntities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in modifiedEntities)
            {
                // Lấy tên thực thể gốc (bỏ qua Proxy class của Entity Framework)
                string tableName = System.Data.Entity.Core.Objects.ObjectContext.GetObjectType(entry.Entity.GetType()).Name;

                if (tableName == "TB_SYS_LOG" || tableName == "TB_SYS_LOGIN_HISTORY") 
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
                        .Where(pn => !(entry.OriginalValues[pn] is byte[])) // Bỏ qua dữ liệu BLOB (Hình ảnh) để tránh làm nặng Log
                        .ToDictionary(pn => pn, pn => entry.OriginalValues[pn]);
                    oldData = JsonConvert.SerializeObject(originalValues);
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var currentValues = entry.CurrentValues.PropertyNames
                        .Where(pn => !(entry.CurrentValues[pn] is byte[])) // Bỏ qua dữ liệu BLOB (Hình ảnh)
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
                        if (originalVal is byte[] || currentVal is byte[]) continue;

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
                    string currentIp = GetLocalIPAddress();
                    string currentMac = GetMacAddress();
                    string pcName = Environment.MachineName;
                    string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    string moduleName = GetModuleName(tableName);

                    string sql = @"INSERT INTO HR.TB_SYS_LOG 
                                  (MANV_THUCHIEN, TEN_THUCHIEN, HANHDONG, TEN_BANG, ID_BAN_GHI, DU_LIEU_CU, DU_LIEU_MOI, IP_ADDRESS, MAC_ADDRESS, TEN_MAY_TINH, THOIGIAN, SESSION_ID, MODULE_NAME, APP_VERSION, CHANGED_FIELDS) 
                                  VALUES (:p0, :p1, :p2, :p3, :p4, :p5, :p6, :p7, :p8, :p9, CURRENT_TIMESTAMP, :p10, :p11, :p12, :p13)";
                                  
                    this.Database.ExecuteSqlCommand(sql,
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p0", CurrentAuditUserId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p1", CurrentAuditUsername ?? "System"),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p2", action.ToUpper()),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p3", tableName),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p4", recordId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p5", oldData ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p6", newData ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p7", currentIp ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p8", currentMac ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p9", pcName ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p10", CurrentSessionId ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p11", moduleName ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p12", appVersion ?? (object)DBNull.Value),
                        new Oracle.ManagedDataAccess.Client.OracleParameter("p13", changedFieldsStr ?? (object)DBNull.Value));
                }
                catch
                {
                    // Không throw lỗi nếu ghi log thất bại để không gián đoạn app
                }
            }

            return base.SaveChanges();
        }
    }
}
