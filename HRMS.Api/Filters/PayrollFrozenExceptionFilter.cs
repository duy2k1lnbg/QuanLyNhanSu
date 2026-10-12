using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Bu.CLASS_SYSTEM;
namespace HRMS_API.Filters {
 public sealed class PayrollFrozenExceptionFilter : ExceptionFilterAttribute {
  public override void OnException(HttpActionExecutedContext context) {
   if(!ErrorHelper.IsPayrollFrozen(context.Exception))return;
   context.Response=context.Request.CreateResponse(HttpStatusCode.Conflict,new {success=false,code="PAYROLL_FROZEN",message=ErrorHelper.PayrollFrozenMessage});
  }
 }
}
