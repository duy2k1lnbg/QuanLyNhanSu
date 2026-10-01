using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Bu.CLASS_CHAMCONG;
class UatPublishingChecks {
 static decimal Scalar(OracleConnection c,string sql){using(var q=c.CreateCommand()){q.CommandText=sql;return Convert.ToDecimal(q.ExecuteScalar());}}
 static void Exec(OracleConnection c,string sql){using(var q=c.CreateCommand()){q.CommandText=sql;q.ExecuteNonQuery();}}
 static void Check(bool ok,string name){if(!ok)throw new Exception("FAIL "+name);Console.WriteLine("PASS "+name);}
 static int Main() {
  string cs=Environment.GetEnvironmentVariable("HRMS_UAT_CONNECTION");
  using(var db=new OracleConnection(cs)){
   db.Open();using(var q=db.CreateCommand()){q.CommandText="SELECT USER FROM DUAL";if((string)q.ExecuteScalar()!="HR_UAT_200")throw new Exception("UAT owner required");}
   var svc=new AttendancePublishingService(cs);
   string outside="SELECT SUM(IDBANGCONGCT*NVL(LANTINH_ID_HIENHANH,0)) FROM TB_BANGCONG_CHITIET WHERE MAKYCONG=202609 AND NOT(MANV=4 AND NGAY=DATE '2026-09-21')";
   decimal before=Scalar(db,outside);
   var request=new AttendancePublishRequest{MaKyCong=202609,TuNgay=new DateTime(2026,9,21),DenNgay=new DateTime(2026,9,22),ManvList=new List<long>{4},NguoiThucHien=1,MaYeuCau="UAT_SCOPE_ONE_DAY"};
   var r=svc.PublishAttendance(request);
   Check(r.Success && r.SoNgayCong==1,"One-day request publishes exactly one day");
   Check(before==Scalar(db,outside),"No current pointers changed outside requested day/employee");
   decimal runs=Scalar(db,"SELECT COUNT(*) FROM TB_CHAMCONG_LANTINH");
   var retry=svc.PublishAttendance(request);
   Check(retry.Success&&retry.IdLanTinh==r.IdLanTinh&&Scalar(db,"SELECT COUNT(*) FROM TB_CHAMCONG_LANTINH")==runs,"Retry returns same version without duplicate run");
   request.DenNgay=new DateTime(2026,9,23);bool rejected=false;
   try{svc.PublishAttendance(request);}catch(InvalidOperationException){rejected=true;}
   Check(rejected,"Same request key with different scope is rejected");
   decimal locked=Scalar(db,"SELECT KHOA FROM TB_KYCONG WHERE MAKYCONG=202607");
   try{
    Exec(db,"UPDATE TB_KYCONG SET KHOA=1 WHERE MAKYCONG=202607");
    var blocked=svc.PublishAttendance(new AttendancePublishRequest{MaKyCong=202607,NguoiThucHien=1,ManvList=new List<long>{1},MaYeuCau="UAT_LOCKED_ATTEMPT"});
    Check(!blocked.Success && Scalar(db,"SELECT COUNT(*) FROM TB_CHAMCONG_LANTINH")==runs,"Locked period rejects publication without new run");
   } finally {Exec(db,"UPDATE TB_KYCONG SET KHOA="+locked.ToString(System.Globalization.CultureInfo.InvariantCulture)+" WHERE MAKYCONG=202607");}
   Check(Scalar(db,"SELECT COUNT(*) FROM TB_CHAMCONG_KQ_NGAY k JOIN TB_CHAMCONG_LANTINH r ON k.IDLANTINH=r.IDLANTINH WHERE k.NGAY<r.TU_NGAY OR k.NGAY>=r.DEN_NGAY")==0,"Every persisted day lies inside its run scope");
   Check(Scalar(db,"SELECT COUNT(*) FROM TB_CONG_PHANDOAN WHERE KETTHUC_LUC>TRUNC(BATDAU_LUC)+1 OR THOILUONG_GIAY<>ROUND((KETTHUC_LUC-BATDAU_LUC)*86400)")==0,"Every physical segment has valid duration and midnight boundary");
  }
  return 0;
 }
}