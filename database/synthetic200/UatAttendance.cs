using System;
using System.Linq;
using System.Globalization;
using Bu.CLASS_CHAMCONG;
using Oracle.ManagedDataAccess.Client;
class UatAttendance {
 static int Main(string[] args) {
  var cs=Environment.GetEnvironmentVariable("HRMS_UAT_CONNECTION");
  if(string.IsNullOrWhiteSpace(cs)) throw new Exception("Explicit UAT connection required.");
  using(var db=new OracleConnection(cs)) {
   db.Open();using(var cmd=db.CreateCommand()){
    cmd.CommandText="SELECT USER FROM DUAL";
    if((string)cmd.ExecuteScalar()!="HR_UAT_200") throw new Exception("UAT-only runner.");
    cmd.CommandText="SELECT COUNT(*) FROM TB_NHANVIEN";
    if(Convert.ToInt32(cmd.ExecuteScalar())!=200) throw new Exception("Expected exactly 200 synthetic employees.");
   }
  }
  System.Threading.Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("vi-VN");
  var service=new AttendancePublishingService(cs);
  foreach(var month in new[]{202607,202608,202609}) {
   Console.WriteLine("PUBLISH_BEGIN "+month);
   var r=service.PublishAttendance(new AttendancePublishRequest{
    MaKyCong=month,NguoiThucHien=1,ManvList=Enumerable.Range(1,200).Select(x=>(long)x).ToList(),
    MaYeuCau="UAT200_"+month+"_V2",GhiChu="Synthetic fixture only - not for payment"
   });
   Console.WriteLine("PUBLISH_RESULT period="+month+" ok="+r.Success+" days="+r.SoNgayCong+" ready="+r.SoNgaySanSang+" pending="+r.SoNgayChoXacMinh+" error="+r.ErrorMessage);
   if(!r.Success) return 1;
  }
  return 0;
 }
}
