using System;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using Bu.CLASS_PAYROLL;
using Oracle.ManagedDataAccess.Client;
using DA;
class UatPayroll {
 static int Main() {
  string cs=Environment.GetEnvironmentVariable("HRMS_UAT_CONNECTION");
  using(var db=new OracleConnection(cs)){db.Open();using(var cmd=db.CreateCommand()){
    cmd.CommandText="SELECT USER FROM DUAL";
    if((string)cmd.ExecuteScalar()!="HR_UAT_200") throw new Exception("UAT-only runner.");
  }}
  MyEntities.GlobalConnectionString=new EntityConnectionStringBuilder {
   Metadata="res://*/QLNhanSu.csdl|res://*/QLNhanSu.ssdl|res://*/QLNhanSu.msl",
   Provider="Oracle.ManagedDataAccess.Client",ProviderConnectionString=cs
  }.ConnectionString;
  if(Environment.GetEnvironmentVariable("HRMS_UAT_CHECK_ONLY")=="1") {
   using(var ctx=new MyEntities()) {
    var loader=typeof(PayrollEngine).GetMethod("LoadEmployeePayrollInput",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
    object[] args={ctx,1m,202607m,2026,7,9500000m,26m};
    loader.Invoke(null,args);
    Console.WriteLine("PASS current attendance accepted");
    using(var tx=ctx.Database.BeginTransaction()) {
     ctx.Database.ExecuteSqlCommand("UPDATE TB_KYCONG SET CONG_INPUT_REV=CONG_INPUT_REV+1 WHERE MAKYCONG=202607");
     bool blocked=false;
     try { loader.Invoke(null,args); }
     catch(System.Reflection.TargetInvocationException e) {
      if(e.InnerException is InvalidOperationException && e.InnerException.Message.Contains("stale")) blocked=true;
      else throw;
     }
     if(!blocked) throw new Exception("Stale attendance was accepted");
     tx.Rollback();
     Console.WriteLine("PASS stale attendance rejected; revision rollback complete");
    }
   }
   return 0;
  }
  var engine=new PayrollEngine();
  var selected=Environment.GetEnvironmentVariable("HRMS_UAT_MONTH");
  foreach(int month in string.IsNullOrEmpty(selected)?new[]{7,8,9}:new[]{int.Parse(selected)}) {
   Console.WriteLine("PAYROLL_BEGIN "+month);
   var r=engine.ExecuteFullPayrollRecalculation(2026,month,"UAT_SYNTHETIC_ONLY");
   Console.WriteLine("PAYROLL_RESULT month="+month+" total="+r.TOTAL_EMPLOYEES+" success="+r.SUCCESS_COUNT+" errors="+r.ERROR_COUNT+" status="+r.STATUS);
   int expected=month==9?7:0;
   if(r.ERROR_COUNT!=expected) return 1;
  }
  return 0;
 }
}