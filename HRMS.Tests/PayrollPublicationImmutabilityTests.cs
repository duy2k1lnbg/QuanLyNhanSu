using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using HRMS_API.Filters;
using NUnit.Framework;
namespace HRMS.Tests {
 [TestFixture] public class PayrollPublicationImmutabilityTests {
  private static PayrollPublicationReview Review(PayrollPublicationRow row,bool locked=true) =>
   (PayrollPublicationReview)typeof(PayrollPublicationService).GetMethod("BuildReview",BindingFlags.NonPublic|BindingFlags.Static)
    .Invoke(null,new object[]{202610,locked,true,new List<PayrollPublicationRow>{row}});
  private static PayrollPublicationRow Valid()=>new PayrollPublicationRow{IDBL=1,MANV=1,STATUS="CALCULATED",RUN_ID=1,RUN_STATUS="SUCCESS",RUN_ERRORS=0,CALCULATED_AT=DateTime.Now,GROSS=1000,NET=850,INSURANCE=100,UNION_FEE=50,SOURCES=1,INPUT_AT_CALC=2,PUBLISH_AT_CALC=2,CURRENT_INPUT=2,CURRENT_PUBLISH=2};
  [Test] public void ModernPublicationRequiresLockedMatchingRevisionAndReconciledNet() {
   var row=Valid();Assert.That(Review(row).CanPublish,Is.True);
   Assert.That(Review(row,false).BlockerCodes,Does.Contain("LOCK_PERIOD"));
   row.CURRENT_INPUT=3;Assert.That(Review(row).BlockerCodes,Does.Contain("INPUT_CHANGED"));
   row.CURRENT_INPUT=2;row.NET=900;Assert.That(Review(row).BlockerCodes,Does.Contain("INVALID_TOTALS"));
   row.NET=850;row.INPUT_AT_CALC=null;Assert.That(Review(row).BlockerCodes,Does.Contain("CALCULATION_REQUIRED"));
  }
  [TestCase("PUBLISHED")][TestCase("APPROVED")][TestCase("CONG_BO")][TestCase("DA_DUYET")][TestCase("PAID")][TestCase("DA_CHI_TRA")]
  public void NoPublishedStateOffersRevokeOrRepublish(string status) {
   var row=Valid();row.STATUS=status;var result=Review(row);
   Assert.That(result.Published,Is.EqualTo(1));Assert.That(result.CanPublish,Is.False);Assert.That(result.CanRevoke,Is.False);
  }
  [Test] public void ArchiveRequiresExplicitLegacyFlagAndCannotRevoke() {
   var row=Valid();row.STATUS="LEGACY_READONLY";row.LEGACY=0;Assert.That(Review(row).CanPublish,Is.False);
   row.LEGACY=1;Assert.That(Review(row,false).CanPublish,Is.True);Assert.That(Review(row,false).CanRevoke,Is.False);
  }
  [Test] public void RevokeCannotReachSqlEvenWithAnOtherwiseValidCommand() {
   var method=typeof(PayrollPublicationService).GetMethod("Apply",BindingFlags.NonPublic|BindingFlags.Static);
   var ex=Assert.Throws<TargetInvocationException>(()=>method.Invoke(null,new object[]{null,null,202610,null,new PayrollPublicationCommand(),true}));
   Assert.That(ErrorHelper.IsPayrollFrozen(ex),Is.True);Assert.That(ErrorHelper.ResolveUserFriendlyMessage(ex),Is.EqualTo(ErrorHelper.PayrollFrozenMessage));
  }
  [Test] public void WrappedFrozenErrorsKeepTheNoticeButOrdinaryErrorsAreNotMisclassified() {
   var wrapped=new AggregateException(new Exception("outer",new BusinessException("PAYROLL_FROZEN","internal details")));
   Assert.That(ErrorHelper.IsPayrollFrozen(wrapped),Is.True);
   Assert.That(ErrorHelper.ResolveUserFriendlyMessage(wrapped),Is.EqualTo(ErrorHelper.PayrollFrozenMessage));
   Assert.That(ErrorHelper.IsPayrollFrozen(new InvalidOperationException("unrelated")),Is.False);
  }
  [Test] public void ApiFrozenConflictHasStableCodeAndNoSqlDetails() {
   using(var config=new HttpConfiguration())using(var request=new HttpRequestMessage(HttpMethod.Post,"https://localhost/api/test")) {
    request.SetConfiguration(config);
    var action=new HttpActionContext{ControllerContext=new HttpControllerContext{Request=request,Configuration=config}};
    var context=new HttpActionExecutedContext(action,new BusinessException("PAYROLL_FROZEN","SQL details must not leak"));
    new PayrollFrozenExceptionFilter().OnException(context);
    Assert.That(context.Response.StatusCode,Is.EqualTo(HttpStatusCode.Conflict));
    var body=context.Response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
    Assert.That(body,Does.Contain("PAYROLL_FROZEN"));Assert.That(body,Does.Not.Contain("SQL details"));
   }
  }
 }
}
