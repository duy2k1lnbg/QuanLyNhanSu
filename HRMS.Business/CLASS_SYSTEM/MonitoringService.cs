using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DA;
using Oracle.ManagedDataAccess.Client;
namespace Bu.CLASS_SYSTEM
{
    public sealed class MonitoringFilter
    {
        public string Kind="activity", Channel, Search, Action, Module, Result, SessionId, CorrelationId, TableName;
        public DateTime? FromDate, ToDate;
        public int Page=1, PageSize=20;
    }
    public sealed class MonitoringRow
    {
        public string id {get;set;} public decimal? userId {get;set;} public string username {get;set;}
        public string channel {get;set;} public string deviceId {get;set;} public string computerName {get;set;}
        public string ipAddress {get;set;} public string userAgent {get;set;} public string timestamp {get;set;}
        public string action {get;set;} public string result {get;set;} public string module {get;set;}
        public string tableName {get;set;} public string recordId {get;set;} public string sessionId {get;set;}
        public string correlationId {get;set;} public string changedFields {get;set;}
        public string oldData {get;set;} public string newData {get;set;} public string appVersion {get;set;}
        public string executor {get;set;} public string ipSource {get;set;} public string lastSeen {get;set;}
        public string expiresAt {get;set;} public bool canRevoke {get;set;}
    }
    public sealed class MonitoringPage
    {
        public bool success=true; public int total; public List<MonitoringRow> data=new List<MonitoringRow>();
    }
    public static class MonitoringService
    {
        public static string NormalizeKind(string kind) => kind=="security" || kind=="sessions" ? kind : "activity";
        public static string NormalizeChannel(string channel) => new[]{"DESKTOP","WEB","MOBILE","SYSTEM","UNKNOWN"}.Contains((channel??"").ToUpperInvariant()) ? channel.ToUpperInvariant() : null;
        public static MonitoringPage Get(MonitoringFilter filter)
        {
            var f=filter??new MonitoringFilter(); f.Kind=NormalizeKind(f.Kind);
            f.Page=Math.Max(1,f.Page); f.PageSize=Math.Max(1,Math.Min(100,f.PageSize));
            using(var db=new MyEntities()) {
                var conn=(OracleConnection)db.Database.Connection; conn.Open();
                string source;
                if(f.Kind=="sessions") source=@"SELECT s.SESSION_ID ID,s.USER_ID USER_ID,u.USERNAME USERNAME,s.CLIENT_TYPE CHANNEL,
                    s.DEVICE_ID_HASH DEVICE_ID,s.DEVICE_NAME DEVICE_NAME,s.IP_ADDRESS IP_ADDRESS,s.USER_AGENT USER_AGENT,
                    s.CREATED_AT STAMP,'SESSION' ACTION,
                    CASE WHEN s.REVOKED_AT IS NOT NULL THEN 'REVOKED' WHEN s.EXPIRES_AT<=CURRENT_TIMESTAMP THEN 'EXPIRED' ELSE 'ACTIVE' END RESULT,
                    'SECURITY' MODULE_NAME,'TB_AUTH_SESSION' TABLE_NAME,s.SESSION_ID RECORD_ID,s.SESSION_ID SESSION_ID,
                    NULL CORRELATION_ID,s.REVOKE_REASON DETAILS,s.LAST_USED_AT LAST_SEEN,s.EXPIRES_AT EXPIRES_AT FROM TB_AUTH_SESSION s
                    LEFT JOIN TB_SYS_USER u ON u.IDUSER=s.USER_ID";
                else if(f.Kind=="security") source=@"SELECT TO_CHAR(a.AUDIT_ID) ID,NVL(a.ACTOR_USER_ID,a.USER_ID) USER_ID,u.USERNAME USERNAME,
                    NVL(CAST(a.CLIENT_TYPE AS VARCHAR2(20)),NVL(CAST(s.CLIENT_TYPE AS VARCHAR2(20)),'UNKNOWN')) CHANNEL,NVL(CAST(a.DEVICE_ID_HASH AS VARCHAR2(200)),CAST(s.DEVICE_ID_HASH AS VARCHAR2(200))) DEVICE_ID,s.DEVICE_NAME DEVICE_NAME,NVL(CAST(a.IP_ADDRESS AS VARCHAR2(64)),CAST(s.IP_ADDRESS AS VARCHAR2(64))) IP_ADDRESS,NVL(CAST(a.USER_AGENT AS NVARCHAR2(1000)),CAST(s.USER_AGENT AS NVARCHAR2(1000))) USER_AGENT,
                    a.OCCURRED_AT STAMP,a.EVENT_TYPE ACTION,a.RESULT RESULT,'SECURITY' MODULE_NAME,'TB_AUTH_AUDIT' TABLE_NAME,
                    TO_CHAR(a.USER_ID) RECORD_ID,a.SESSION_ID SESSION_ID,a.CORRELATION_ID CORRELATION_ID,a.REASON DETAILS,
                    NULL LAST_SEEN,NULL EXPIRES_AT FROM TB_AUTH_AUDIT a LEFT JOIN TB_SYS_USER u ON u.IDUSER=NVL(a.ACTOR_USER_ID,a.USER_ID) LEFT JOIN TB_AUTH_SESSION s ON s.SESSION_ID=a.SESSION_ID";
                else {
                    bool extended;
                    using(var c=new OracleCommand("SELECT COUNT(*) FROM USER_TAB_COLUMNS WHERE TABLE_NAME='TB_SYS_LOG' AND COLUMN_NAME IN ('CLIENT_TYPE','DEVICE_ID','CORRELATION_ID','EVENT_RESULT','EXECUTOR','IP_SOURCE','USER_AGENT')",conn)) extended=Convert.ToInt32(c.ExecuteScalar())==7;
                    source="SELECT TO_CHAR(l.ID_LOG) ID,l.MANV_THUCHIEN USER_ID,l.TEN_THUCHIEN USERNAME,"+
                        (extended?"NVL(CAST(l.CLIENT_TYPE AS VARCHAR2(20)),NVL(CAST(s.CLIENT_TYPE AS VARCHAR2(20)),'UNKNOWN')) CHANNEL,NVL(l.DEVICE_ID,s.DEVICE_ID_HASH) DEVICE_ID,l.CORRELATION_ID CORRELATION_ID,NVL(l.EVENT_RESULT,'UNKNOWN') RESULT,l.EXECUTOR EXECUTOR,l.IP_SOURCE IP_SOURCE,NVL(l.USER_AGENT,s.USER_AGENT) USER_AGENT,":"NVL(s.CLIENT_TYPE,'UNKNOWN') CHANNEL,s.DEVICE_ID_HASH DEVICE_ID,NULL CORRELATION_ID,'UNKNOWN' RESULT,NULL EXECUTOR,NULL IP_SOURCE,s.USER_AGENT USER_AGENT,")+
                        @"NVL(l.TEN_MAY_TINH,s.DEVICE_NAME) DEVICE_NAME,NVL(l.IP_ADDRESS,s.IP_ADDRESS) IP_ADDRESS,l.THOIGIAN STAMP,l.HANHDONG ACTION,l.MODULE_NAME MODULE_NAME,l.TEN_BANG TABLE_NAME,
                        l.ID_BAN_GHI RECORD_ID,l.SESSION_ID SESSION_ID,l.CHANGED_FIELDS DETAILS,l.DU_LIEU_CU OLD_DATA,l.DU_LIEU_MOI NEW_DATA,l.APP_VERSION APP_VERSION,
                        NULL LAST_SEEN,NULL EXPIRES_AT FROM TB_SYS_LOG l LEFT JOIN TB_AUTH_SESSION s ON s.SESSION_ID=l.SESSION_ID";
                }
                string where=" WHERE 1=1"; var args=new Dictionary<string,object>();
                Action<string,string,object> add=(clause,name,value)=>{where+=clause;args[name]=value;};
                var channel=NormalizeChannel(f.Channel); if(channel!=null) add(" AND UPPER(CHANNEL)=:p_channel","p_channel",channel);
                if(!string.IsNullOrWhiteSpace(f.Result) && f.Result!="ALL") add(" AND UPPER(RESULT)=:p_result","p_result",f.Result.ToUpperInvariant());
                if(!string.IsNullOrWhiteSpace(f.Action) && f.Action!="ALL") add(" AND UPPER(ACTION)=:p_action","p_action",f.Action.ToUpperInvariant());
                if(!string.IsNullOrWhiteSpace(f.Module) && f.Module!="ALL") add(" AND UPPER(MODULE_NAME)=:p_module","p_module",f.Module.ToUpperInvariant());
                if(!string.IsNullOrWhiteSpace(f.TableName)) add(" AND UPPER(TABLE_NAME)=:p_table","p_table",f.TableName.Trim().ToUpperInvariant());
                if(!string.IsNullOrWhiteSpace(f.SessionId)) add(" AND SESSION_ID=:p_session","p_session",f.SessionId.Trim());
                if(!string.IsNullOrWhiteSpace(f.CorrelationId)) add(" AND CORRELATION_ID=:p_correlation","p_correlation",f.CorrelationId.Trim());
                if(f.FromDate.HasValue) add(" AND STAMP>=:p_startDate","p_startDate",f.FromDate.Value.Date);
                if(f.ToDate.HasValue) add(" AND STAMP<:p_endDate","p_endDate",f.ToDate.Value.Date.AddDays(1));
                if(!string.IsNullOrWhiteSpace(f.Search)) add(" AND (LOWER(USERNAME) LIKE :p_search OR LOWER(DEVICE_NAME) LIKE :p_search OR LOWER(IP_ADDRESS) LIKE :p_search OR LOWER(RECORD_ID) LIKE :p_search OR LOWER(ACTION) LIKE :p_search)","p_search","%"+f.Search.Trim().ToLowerInvariant()+"%");
                var response=new MonitoringPage();
                using(var c=Command(conn,"SELECT COUNT(*) FROM ("+source+")"+where,args)) response.total=Convert.ToInt32(c.ExecuteScalar());
                args["p_startRow"]=(f.Page-1)*(long)f.PageSize;args["p_endRow"]=(long)f.Page*f.PageSize;
                using(var c=Command(conn,"SELECT * FROM (SELECT q.*,ROW_NUMBER() OVER (ORDER BY STAMP DESC,ID DESC) RN FROM ("+source+") q"+where+") WHERE RN>:p_startRow AND RN<=:p_endRow",args))
                using(var r=c.ExecuteReader()) {
                    while(r.Read()) {
                        Func<string,string> value=name=> {try{return r[name]==DBNull.Value?null:Convert.ToString(r[name]);}catch(IndexOutOfRangeException){return null;}};
                        Func<string,string> date=name=>r[name]==DBNull.Value?null:Convert.ToDateTime(r[name]).ToString("yyyy-MM-dd HH:mm:ss");
                        var row=new MonitoringRow {id=value("ID"),userId=r["USER_ID"]==DBNull.Value?(decimal?)null:Convert.ToDecimal(r["USER_ID"]),username=value("USERNAME")??"System",
                            channel=NormalizeChannel(value("CHANNEL"))??"UNKNOWN",deviceId=value("DEVICE_ID"),computerName=value("DEVICE_NAME"),ipAddress=value("IP_ADDRESS"),userAgent=value("USER_AGENT"),
                            timestamp=date("STAMP"),action=value("ACTION"),result=value("RESULT"),module=value("MODULE_NAME"),tableName=value("TABLE_NAME"),recordId=value("RECORD_ID"),sessionId=value("SESSION_ID"),
                            correlationId=value("CORRELATION_ID"),changedFields=AuditIdentity.RedactJson(value("DETAILS")),oldData=AuditIdentity.RedactJson(value("OLD_DATA")),newData=AuditIdentity.RedactJson(value("NEW_DATA")),
                            appVersion=value("APP_VERSION"),executor=value("EXECUTOR"),ipSource=value("IP_SOURCE"),lastSeen=date("LAST_SEEN"),expiresAt=date("EXPIRES_AT")};
                        response.data.Add(row);
                    }
                }
                return response;
            }
        }
        private static OracleCommand Command(OracleConnection conn,string sql,IDictionary<string,object> args)
        {
            var c=new OracleCommand(sql,conn){BindByName=true};foreach(var pair in args)c.Parameters.Add(pair.Key,pair.Value??DBNull.Value);return c;
        }
    }
}
