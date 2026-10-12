using System;
using System.Configuration;
using System.Linq;
using System.Web;
using DA;
using Oracle.ManagedDataAccess.Client;
namespace HRMS_API.Services
{
    public static class AuditRequestContext
    {
        public static string ClientIp(HttpContextBase context)
        {
            if(context==null) return null;
            string peer=context.Request.UserHostAddress;
            var trusted=(ConfigurationManager.AppSettings["Audit.TrustedProxies"]??"").Split(',').Select(x=>x.Trim());
            if(trusted.Contains(peer)) {
                var chain=(context.Request.Headers["X-Forwarded-For"]??"").Split(',').Select(x=>x.Trim()).Where(x=>x.Length>0).ToArray();
                foreach(var candidate in chain.Reverse()) {
                    System.Net.IPAddress parsed;
                    if(!System.Net.IPAddress.TryParse(candidate,out parsed)) return peer;
                    if(!trusted.Contains(candidate)) return candidate;
                }
            }
            return peer;
        }
        public static AuditIdentity Create(JwtUserClaims claims,HttpContextBase context)
        {
            var id=new AuditIdentity {UserId=int.Parse(claims.UserId),Username=claims.Username,Channel=claims.ClientType,
                IpAddress=ClientIp(context),IpSource="SERVER_OBSERVED",Executor="API",CorrelationId=Guid.NewGuid().ToString("N"),UserAgent=context?.Request.UserAgent};
            using(var db=new MyEntities()) {
                var conn=(OracleConnection)db.Database.Connection;conn.Open();
                using(var cmd=new OracleCommand("SELECT SESSION_ID,DEVICE_ID_HASH,DEVICE_NAME FROM TB_AUTH_SESSION WHERE USER_ID=:userId AND JTI=:jti",conn){BindByName=true}) {
                    cmd.Parameters.Add("userId",id.UserId);cmd.Parameters.Add("jti",claims.Jti);
                    using(var r=cmd.ExecuteReader()) if(r.Read()) {id.SessionId=Convert.ToString(r[0]);id.DeviceId=Convert.ToString(r[1]);id.DeviceName=Convert.ToString(r[2]);}
                }
            }
            return id;
        }
    }
}
