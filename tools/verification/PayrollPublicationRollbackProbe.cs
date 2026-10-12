using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Http;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using DA;
using HRMS_API.Controllers;
using HRMS_API.Services;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
class PayrollPublicationProbe {
 static int passed;
 static object Invoke(string method,params object[] args){try{return typeof(PayrollPublicationService).GetMethod(method,BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,args);}catch(TargetInvocationException ex){throw ex.InnerException;}}
 static void Check(bool ok,string name){if(!ok)throw new Exception("Check failed: "+name);passed++;Console.WriteLine("PASS "+name);}
 static void Reject(Action action,Type type,string name){try{action();}catch(Exception ex){Check(type.IsInstanceOfType(ex),name);return;}throw new Exception("Expected rejection: "+name);}
 static object Scalar(OracleConnection conn,OracleTransaction tx,string sql,params object[] values){using(var cmd=new OracleCommand(sql,conn)){cmd.Transaction=tx;cmd.BindByName=true;for(int i=0;i<values.Length;i++)cmd.Parameters.Add("p"+i,values[i]??DBNull.Value);return cmd.ExecuteScalar();}}
 static string Fingerprint(OracleConnection conn,OracleTransaction tx,int period,bool includeStatus){
  var values=new List<string>();using(var cmd=new OracleCommand("SELECT IDBL,"+(includeStatus?"TRANG_THAI,":"")+"TONG_CONG,THUC_LINH,TIEN_BHXH_TRICH,TIEN_CONG_DOAN,TIEN_TAMUNG,THUE_TNCN,KHOAN_TRU_KHAC,HOAN_THUE FROM TB_BANGLUONG WHERE MAKYCONG=:p0 ORDER BY IDBL",conn)){
   cmd.Transaction=tx;cmd.Parameters.Add("p0",period);using(var r=cmd.ExecuteReader())while(r.Read())for(int i=0;i<r.FieldCount;i++)values.Add(Convert.ToString(r[i],CultureInfo.InvariantCulture));
  }using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(string.Join("|",values))));
 }
 static BangLuongController Controller(decimal id,string channel){var config=new HttpConfiguration();var request=new HttpRequestMessage(HttpMethod.Get,"https://localhost/api/bangluong");request.SetConfiguration(config);request.Properties["JwtUser"]=new JwtUserClaims{UserId=id.ToString(),ClientType=channel};return new BangLuongController{Configuration=config,Request=request};}
 static void Status(IHttpActionResult action,int expected,string name){using(var response=action.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult()){Check((int)response.StatusCode==expected,name);}}
 static int Main(){try{
  var cfg=JObject.Parse(File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"QLyNSu_DBConfig.json")));
  var p=cfg["Profiles"].First(x=>(string)x["Id"]==(string)cfg["ActiveProfileId"]);
  var cs=new OracleConnectionStringBuilder{UserID=(string)p["Username"],Password=(string)p["Password"],DataSource=(string)p["ServerIP"]+":"+(string)p["Port"]+"/"+(string)p["Database"]};
  MyEntities.GlobalConnectionString=new EntityConnectionStringBuilder{Metadata="res://*/QLNhanSu.csdl|res://*/QLNhanSu.ssdl|res://*/QLNhanSu.msl",Provider="Oracle.ManagedDataAccess.Client",ProviderConnectionString=cs.ConnectionString}.ConnectionString;
  decimal admin;using(var db=new MyEntities())admin=db.TB_SYS_USER.Single(u=>u.USERNAME=="ADMIN").IDUSER;
  var service=new PayrollPublicationService();
  Reject(()=>service.Review(202613,admin,"DESKTOP"),typeof(ArgumentException),"invalid period rejected");
  Reject(()=>service.Review(202601,admin,"MOBILE"),typeof(UnauthorizedAccessException),"employee portal cannot publish or inspect admin publication");
  Reject(()=>service.Publish(202601,21,"DESKTOP",new PayrollPublicationCommand{Reason="TEST_VALID_REASON",ExpectedStamp="stale"}),typeof(UnauthorizedAccessException),"employee without publication permission cannot write");
  var modern=new PayrollPublicationRow{IDBL=1,MANV=1,STATUS="CALCULATED",RUN_ID=1,RUN_STATUS="SUCCESS",RUN_ERRORS=0,CALCULATED_AT=DateTime.Now,GROSS=1000,NET=850,INSURANCE=100,UNION_FEE=50,SOURCES=1,INPUT_AT_CALC=2,PUBLISH_AT_CALC=2,CURRENT_INPUT=2,CURRENT_PUBLISH=2};
  Func<bool,PayrollPublicationReview> build=locked=>(PayrollPublicationReview)Invoke("BuildReview",202610,locked,true,new List<PayrollPublicationRow>{modern});
  Check(build(true).CanPublish,"valid modern snapshot and locked period eligible");
  Check(!build(false).CanPublish && build(false).BlockerCodes.Contains("LOCK_PERIOD"),"open modern period blocks publication");
  modern.CURRENT_INPUT=3;Check(build(true).BlockerCodes.Contains("INPUT_CHANGED"),"attendance revision changed blocks publication");modern.CURRENT_INPUT=2;
  modern.INPUT_AT_CALC=null;Check(build(true).BlockerCodes.Contains("CALCULATION_REQUIRED"),"old calculation without revision snapshot must be recalculated");modern.INPUT_AT_CALC=2;
  modern.NET=900;Check(build(true).BlockerCodes.Contains("INVALID_TOTALS"),"gross minus deductions must match net");modern.NET=850;
  modern.STATUS="PAID";Check(!build(true).CanRevoke,"paid slips cannot be retracted");modern.STATUS="CALCULATED";
  var current=service.Review(202610,admin,"DESKTOP");Check(!current.CanPublish && current.BlockerCodes.Contains("CALCULATION_REQUIRED"),"actual demo calculation without source revision is blocked");
  Status(Controller(admin,"WEB").GetPublication(202601),200,"Web review reads authorized administrator");
  Status(Controller(admin,"MOBILE").GetPublication(202601),403,"API rejects wrong token channel independently of UI");
  Status(Controller(admin,"WEB").Publish(202601,new PayrollPublicationCommand{Reason="TEST_VALID_REASON",ExpectedStamp="stale"}),409,"API returns friendly conflict for stale review, no commit");
  using(var db=new MyEntities())using(var conn=new OracleConnection(cs.ConnectionString)){
   conn.Open();var actor=Invoke("Authorize",db,admin,"DESKTOP",true);int period=202601;
   var before=Fingerprint(conn,null,period,true);var amounts=Fingerprint(conn,null,period,false);
   decimal notificationsBefore=Convert.ToDecimal(Scalar(conn,null,"SELECT COUNT(*) FROM TB_THONGBAO WHERE EVENT_TYPE IN ('PAYROLL_PUBLISHED','PAYROLL_PUBLICATION_REVOKED')"));
   using(var tx=conn.BeginTransaction()){
    try{
     decimal slip=Convert.ToDecimal(Scalar(conn,tx,"SELECT MIN(IDBL) FROM TB_BANGLUONG WHERE MAKYCONG=:p0",period));
     decimal employee=Convert.ToDecimal(Scalar(conn,tx,"SELECT MANV FROM TB_BANGLUONG WHERE IDBL=:p0",slip));
     Action<string,object[]> execute=(sql,values)=>{using(var cmd=new OracleCommand(sql,conn)){cmd.Transaction=tx;cmd.BindByName=true;for(int i=0;i<values.Length;i++)cmd.Parameters.Add("p"+i,values[i]);cmd.ExecuteNonQuery();}};
     execute("INSERT INTO TB_BANGCONG_CHITIET(IDBANGCONGCT,MAKYCONG,MANV,NGAY) VALUES(987654301,:p0,:p1,DATE '2026-01-02')",new object[]{period,employee});
     execute("INSERT INTO TB_BANGLUONG_CT(IDBLCT,IDBL,MANV,MAKYCONG,NHOM_KHOAN_MUC,MA_KHOAN_MUC,TEN_KHOAN_MUC,SO_LUONG,DON_GIA,HE_SO,THANH_TIEN,IS_LEGACY) VALUES(987654302,:p0,:p1,:p2,'THU_NHAP','TEST','ROLLBACK_FIXTURE',1,0,1,0,1)",new object[]{slip,employee,period});
     execute("INSERT INTO TB_BANGLUONG_CT_SOURCE(ID,IDBLCT,MANV,SOURCE_TYPE,SOURCE_BCCT_ID,WEIGHT_QUANTITY,AMOUNT_CONTRIBUTED) VALUES(987654303,987654302,:p0,'BANGCONG',987654301,1,0)",new object[]{employee});
     decimal latchBefore=Convert.ToDecimal(Scalar(conn,tx,"SELECT LUONG_CONGBO_LOCKED FROM TB_KYCONG WHERE MAKYCONG=:p0",period));
     execute("SAVEPOINT BEFORE_PUBLICATION",new object[0]);
     var review=(PayrollPublicationReview)Invoke("ReadReview",conn,tx,period,actor);
     Check(review.CanPublish && review.LegacyPending==review.Pending && review.Pending>0,"archived payroll available with explicit historical acknowledgment");
     var command=new PayrollPublicationCommand{ExpectedStamp=review.Stamp,Reason="ROLLBACK_PUBLICATION_PROBE",ConfirmLegacy=false};
     Reject(()=>Invoke("Apply",conn,tx,period,actor,command,false),typeof(InvalidOperationException),"historical acknowledgment required before any mutation");
     Check(Fingerprint(conn,tx,period,true)==before,"rejected acknowledgment leaves all slips unchanged");
     command.ConfirmLegacy=true;
     var change=(PayrollPublicationChange)Invoke("Apply",conn,tx,period,actor,command,false);
     Check(change.Changed==review.Pending,"all scoped archived slips published atomically");
     Check(Fingerprint(conn,tx,period,false)==amounts,"publication preserves every money field");
     var published=(PayrollPublicationReview)Invoke("ReadReview",conn,tx,period,actor);
     Check(published.Pending==0 && published.Published==review.Total && !published.CanRevoke && published.Frozen,"published state exposes slips and permanently freezes the period");
     Check(Convert.ToInt32(Scalar(conn,tx,"SELECT COUNT(*) FROM TB_SYS_LOG WHERE HANHDONG='CONG_BO_BANGLUONG' AND DU_LIEU_MOI LIKE '%ROLLBACK_PUBLICATION_PROBE%'"))==change.Changed,"one audit record per published slip in same transaction");
     Check(Convert.ToDecimal(Scalar(conn,tx,"SELECT COUNT(*) FROM TB_THONGBAO WHERE EVENT_TYPE IN ('PAYROLL_PUBLISHED','PAYROLL_PUBLICATION_REVOKED')"))-notificationsBefore==change.Changed,"publication creates employee notifications in same transaction");
     var repeat=(PayrollPublicationChange)Invoke("Apply",conn,tx,period,actor,command,false);Check(repeat.Changed==0,"repeated publish has no duplicate effects");
     Reject(()=>Invoke("Apply",conn,tx,period,actor,command,true),typeof(BusinessException),"retraction is forbidden even for authorized administrator");
     Action<string,string,object[]> frozen=(sql,label,values)=>{
      try{execute(sql,values);}catch(OracleException ex){
       Check(ex.Number==20044,label);
       Check(ErrorHelper.ResolveUserFriendlyMessage(new Exception("wrapped",ex))==ErrorHelper.PayrollFrozenMessage,"friendly wrapped Oracle notice: "+label);return;
      }throw new Exception("Expected frozen write: "+label);
     };
     frozen("UPDATE TB_BANGLUONG SET THUC_LINH=THUC_LINH+1 WHERE IDBL=:p0","salary amount change rejected",new object[]{slip});
     frozen("UPDATE TB_BANGLUONG SET TRANG_THAI='CALCULATED' WHERE IDBL=:p0","salary status rollback rejected",new object[]{slip});
     frozen("DELETE FROM TB_BANGLUONG WHERE IDBL=:p0","published payslip deletion rejected",new object[]{slip});
     frozen("INSERT INTO TB_BANGLUONG(IDBL,MANV,MAKYCONG,NAM,THANG) VALUES(987654309,:p0,:p1,2026,1)","new slip in frozen period rejected",new object[]{employee,period});
     frozen("UPDATE TB_KYCONG SET KHOA=0 WHERE MAKYCONG=:p0","period unlock rejected",new object[]{period});
     frozen("UPDATE TB_KYCONG SET LUONG_CONGBO_LOCKED=0 WHERE MAKYCONG=:p0","persistent publication latch cannot be reset",new object[]{period});
     frozen("DELETE FROM TB_KYCONG WHERE MAKYCONG=:p0","published period deletion rejected",new object[]{period});
     frozen("UPDATE TB_BANGCONG_CHITIET SET GHICHU='TAMPER' WHERE IDBANGCONGCT=987654301","detailed attendance change rejected",new object[0]);
     frozen("UPDATE TB_BANGCONG_CHITIET SET MAKYCONG=202602 WHERE IDBANGCONGCT=987654301","attendance reparenting cannot escape freeze",new object[0]);
     frozen("DELETE FROM TB_BANGCONG_CHITIET WHERE IDBANGCONGCT=987654301","attendance deletion rejected",new object[0]);
     frozen("UPDATE TB_BANGLUONG_CT SET THANH_TIEN=1 WHERE IDBLCT=987654302","salary component change rejected",new object[0]);
     frozen("UPDATE TB_BANGLUONG_CT_SOURCE SET AMOUNT_CONTRIBUTED=1 WHERE ID=987654303","payroll source amount change rejected",new object[0]);
     frozen("INSERT INTO TB_TANGCA(IDTCA,NAM,THANG,NGAY,MANV,SOGIO) VALUES(987654304,2026,1,2,:p0,1)","overtime injection rejected",new object[]{employee});
     frozen("INSERT INTO TB_UNGLUONG(IDUL,NAM,THANG,NGAY,MANV,SOTIENUNG) VALUES(987654305,2026,1,2,:p0,1000)","advance injection rejected",new object[]{employee});
     frozen("INSERT INTO TB_BANGLUONG_CT(IDBLCT,IDBL,MANV,MAKYCONG,NHOM_KHOAN_MUC,MA_KHOAN_MUC,TEN_KHOAN_MUC) VALUES(987654306,:p0,:p1,202602,'THU_NHAP','TEST','TEST')","forged open-period child cannot alter frozen parent",new object[]{slip,employee});
     Check(Fingerprint(conn,tx,period,false)==amounts,"all denied writes leave amounts unchanged");
     execute("UPDATE TB_BANGLUONG SET THUC_LINH=THUC_LINH WHERE MAKYCONG=202602",new object[0]);
     Check(true,"another unpublished period remains writable");
     execute("ROLLBACK TO SAVEPOINT BEFORE_PUBLICATION",new object[0]);
     Check(Fingerprint(conn,tx,period,true)==before,"publication and payroll statuses roll back together");
     Check(Convert.ToDecimal(Scalar(conn,tx,"SELECT LUONG_CONGBO_LOCKED FROM TB_KYCONG WHERE MAKYCONG=:p0",period))==latchBefore,"publication latch rolls back in same transaction");
     Check(Convert.ToDecimal(Scalar(conn,tx,"SELECT COUNT(*) FROM TB_THONGBAO WHERE EVENT_TYPE IN ('PAYROLL_PUBLISHED','PAYROLL_PUBLICATION_REVOKED')"))==notificationsBefore,"notifications roll back with publication");
     execute("UPDATE TB_BANGCONG_CHITIET SET GHICHU='ROLLBACK_ALLOWED' WHERE IDBANGCONGCT=987654301",new object[0]);
     Check(true,"rollback restores editability of unpublished attendance");
     // Explicit company scope does not expose other companies' records.
     actor.GetType().GetField("Company",BindingFlags.Instance|BindingFlags.Public).SetValue(actor,(decimal?)999999m);
     var scoped=(PayrollPublicationReview)Invoke("ReadReview",conn,tx,period,actor);
     Check(scoped.Total==0 && !scoped.CanPublish && scoped.BlockerCodes.Contains("GLOBAL_PERIOD_SCOPE"),"company scope excludes outside payroll rows");
    }finally{tx.Rollback();}
   }
   Check(Convert.ToInt32(Scalar(conn,null,"SELECT COUNT(*) FROM TB_BANGCONG_CHITIET WHERE IDBANGCONGCT=987654301"))==0,"no attendance fixture remains after full rollback");
   Check(Fingerprint(conn,null,period,true)==before,"database statuses and amounts restored after rollback");
   Check(Convert.ToDecimal(Scalar(conn,null,"SELECT COUNT(*) FROM TB_THONGBAO WHERE EVENT_TYPE IN ('PAYROLL_PUBLISHED','PAYROLL_PUBLICATION_REVOKED')"))==notificationsBefore,"no publication notification committed by test");
  }
  Console.WriteLine("TOTAL_PASS="+passed);return 0;
 }catch(Exception ex){var oracle=ex as OracleException;Console.WriteLine("FAILED "+(oracle!=null?"ORA-"+oracle.Number+" "+oracle.Message:ex.Message));return 1;}}
}
