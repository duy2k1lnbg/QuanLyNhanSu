using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DA;
using Bu.CLASS_SYSTEM;
using Bu.CLASS_SECURITY;
using HRMS_API.Filters;
namespace HRMS_API.Controllers
{
    [JwtAuthorize]
    [RoutePrefix("api/audit")]
    public class AuditController:ApiController
    {
        [HttpGet,Route("")]
        [JwtAuthorize(Right="F_SYSTEM_GIAMSAT", Action=ChannelAction.View)]
        public IHttpActionResult GetAuditLogs(string kind="activity",string channel=null,string result=null,string search=null,string action=null,string module=null,string sessionId=null,string correlationId=null,string tableName=null,DateTime? fromDate=null,DateTime? toDate=null,int page=1,int pageSize=20)
        {
            try {
                var response=MonitoringService.Get(new MonitoringFilter {Kind=kind,Channel=channel,Result=result,Search=search,Action=action,Module=module,SessionId=sessionId,CorrelationId=correlationId,TableName=tableName,FromDate=fromDate,ToDate=toDate,Page=page,PageSize=pageSize});
                var claims=JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
                bool mayRevoke=false; using(var permissionDb=new MyEntities()) mayRevoke=claims!=null && new ChannelPermissionResolver().CanExecuteAction(permissionDb,decimal.Parse(claims.UserId),claims.ClientType,"F_SYSTEM_GIAMSAT",ChannelAction.Edit,claims.Username);
                foreach(var row in response.data) row.canRevoke=kind=="sessions" && row.result=="ACTIVE" && mayRevoke;
                return Ok(response);
            } catch(Exception ex) {System.Diagnostics.Trace.TraceError("Audit query: "+ex.GetType().Name);return Content(HttpStatusCode.InternalServerError,new{success=false,message="Không tải được nhật ký. Vui lòng thử lại."});}
        }
        [HttpGet,Route("{id:decimal}")]
        [JwtAuthorize(Right="F_SYSTEM_GIAMSAT", Action=ChannelAction.View)]
        public IHttpActionResult GetAuditDetail(decimal id)
        {
            try {
                using(var db=new MyEntities()) {
                    var row=db.TB_SYS_LOG.FirstOrDefault(x=>x.ID_LOG==id); if(row==null)return NotFound();
                    return Ok(new{success=true,data=new {id=row.ID_LOG,userId=row.MANV_THUCHIEN,username=row.TEN_THUCHIEN,action=row.HANHDONG,tableName=row.TEN_BANG,recordId=row.ID_BAN_GHI,module=row.MODULE_NAME,
                        ipAddress=row.IP_ADDRESS,computerName=row.TEN_MAY_TINH,timestamp=row.THOIGIAN?.ToString("yyyy-MM-dd HH:mm:ss"),changedFields=AuditIdentity.RedactJson(row.CHANGED_FIELDS),oldData=AuditIdentity.RedactJson(row.DU_LIEU_CU),newData=AuditIdentity.RedactJson(row.DU_LIEU_MOI),appVersion=row.APP_VERSION,sessionId=row.SESSION_ID}});
                }
            } catch {return Content(HttpStatusCode.InternalServerError,new{success=false,message="Không tải được chi tiết nhật ký."});}
        }
        [HttpGet,Route("modules")]
        [JwtAuthorize(Right="F_SYSTEM_GIAMSAT", Action=ChannelAction.View)]
        public IHttpActionResult GetModules()
        {
            using(var db=new MyEntities()) return Ok(new{success=true,modules=db.Database.SqlQuery<string>("SELECT DISTINCT MODULE_NAME FROM TB_SYS_LOG WHERE MODULE_NAME IS NOT NULL").ToList(),actions=db.Database.SqlQuery<string>("SELECT DISTINCT HANHDONG FROM TB_SYS_LOG WHERE HANHDONG IS NOT NULL").ToList()});
        }
        [HttpPost,Route("sessions/{id}/revoke"),JwtAuthorize(Right="F_SYSTEM_GIAMSAT",Action=ChannelAction.Edit)]
        public async Task<IHttpActionResult> Revoke(string id)
        {
            var actor=JwtAuthorizeAttribute.GetCurrentJwtUser(Request);
            using(var db=new MyEntities()) if(db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TB_AUTH_SESSION WHERE SESSION_ID=:p0",new Oracle.ManagedDataAccess.Client.OracleParameter("p0",id)).Single()!=1)return NotFound();
            bool success=await new AuthSecurityService().RevokeSessionAsync(id,"ADMIN_MONITOR_REVOKE",decimal.Parse(actor.UserId),Guid.NewGuid().ToString("N"));
            return success ? (IHttpActionResult)Ok(new{success=true}) : Content(HttpStatusCode.InternalServerError,new{success=false,message="Không thu hồi được phiên."});
        }
    }
}
