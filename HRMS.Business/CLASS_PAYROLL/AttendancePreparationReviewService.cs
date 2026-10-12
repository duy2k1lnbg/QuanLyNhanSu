using Bu.CLASS_CHAMCONG;
using Bu.CLASS_SYSTEM;
using DA;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
namespace Bu.CLASS_PAYROLL
{
    public sealed class AttendancePreparationReviewService
    {
        public sealed class Decision {public string MA_SU_VIEC_HASH {get;set;} public string INPUT_HASH {get;set;} public decimal HISTORY_ID {get;set;} }
        public sealed class CaseRow {public decimal IDBATTHUONG {get;set;} public decimal MANV {get;set;} public decimal IDBANGCONGCT {get;set;} public string MA_LOI {get;set;} public string INPUT_HASH {get;set;} public string MA_SU_VIEC_HASH {get;set;} public string TRANG_THAI {get;set;} public string INPUT_SNAPSHOT {get;set;} }
        public static bool CanExclude(string code)=>code=="OT_CHUA_DUYET" || code=="CHAM_NGOAI_LICH";
        public static List<Decision> LoadDecisions(OracleConnection conn,int period)
        {
            var rows=new List<Decision>();
            using(var cmd=new OracleCommand(@"SELECT a.MA_SU_VIEC_HASH,a.INPUT_HASH,h.ID FROM TB_CHAMCONG_BATTHUONG a
                JOIN TB_CHAMCONG_BT_LICHSU h ON h.IDBATTHUONG=a.IDBATTHUONG AND h.INPUT_HASH_DUYET=a.INPUT_HASH
                JOIN TB_BANGCONG_CHITIET b ON b.IDBANGCONGCT=a.IDBANGCONGCT
                WHERE b.MAKYCONG=:p AND a.TRANG_THAI='KHONG_TINH' AND h.DEN_TRANGTHAI='KHONG_TINH'
                  AND h.THUTU=(SELECT MAX(x.THUTU) FROM TB_CHAMCONG_BT_LICHSU x WHERE x.IDBATTHUONG=a.IDBATTHUONG)",conn)) {
                cmd.BindByName=true;cmd.Parameters.Add("p",period);using(var r=cmd.ExecuteReader())while(r.Read())rows.Add(new Decision {MA_SU_VIEC_HASH=r.GetString(0),INPUT_HASH=r.GetString(1),HISTORY_ID=Convert.ToDecimal(r[2])});
            }return rows;
        }
        // A decision can only acknowledge an unpaid outside interval with exactly matching evidence.
        // It never awards wages, erases device punches or waives missing configuration.
        public static void ApplyDecisions(DayCalculationResult day,IEnumerable<Decision> decisions)
        {
            var applied=new List<Decision>();
            foreach(var anomaly in day.Anomalies.Where(a=>CanExclude(a.MaLoi))) {
                var decision=decisions.FirstOrDefault(d=>d.MA_SU_VIEC_HASH==anomaly.MaSuViecHash && d.INPUT_HASH==anomaly.InputHash && d.HISTORY_ID>0);
                if(decision==null)continue;anomaly.TrangThai="KHONG_TINH";applied.Add(decision);
                foreach(var segment in day.Segments.Where(s=>s.TrangThaiXacNhan=="CHO_XAC_MINH" && s.GiayHuongCongThuong==0 && s.GiayOtXacNhan==0 && anomaly.BatDauLuc.HasValue && anomaly.KetThucLuc.HasValue && s.BatDauLuc>=anomaly.BatDauLuc && s.KetThucLuc<=anomaly.KetThucLuc))segment.TrangThaiXacNhan="KHONG_TINH";
            }
            if(applied.Count==0)return;
            if(day.Anomalies.All(a=>a.TrangThai!="CHO_XU_LY")) {day.DuDieuKienChot=1;day.TrangThaiCong="DA_XAC_NHAN";}
            var snapshot=JObject.Parse(day.InputSnapshotJson);snapshot["ReviewedUnpaidCases"]=JArray.FromObject(applied.OrderBy(d=>d.HISTORY_ID));day.InputSnapshotJson=JsonConvert.SerializeObject(snapshot,Formatting.None);
            using(var sha=SHA256.Create())day.InputHash=string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(day.InputSnapshotJson)).Select(b=>b.ToString("x2")));
        }
        public void ExcludeConfirmedInterval(decimal caseId,int period,string reason)
        {
            if(!UserSession.CanView("F_NV_PHEDUYET") || !UserSession.CanEdit("F_NV_PHEDUYET"))throw new UnauthorizedAccessException("Cần quyền phê duyệt yêu cầu để xác nhận khoảng ngoài ca.");
            if(string.IsNullOrWhiteSpace(reason))throw new ArgumentException("Nhập căn cứ xác minh việc không tính khoảng thời gian này.");
            using(var db=new MyEntities())using(var tx=db.Database.BeginTransaction()) {
                var locks=PayrollPreparationSql.Query<decimal>(db,"SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p FOR UPDATE",new OracleParameter("p",period));
                if(locks.Count!=1 || locks[0]!=0)throw new InvalidOperationException("Kỳ đã khóa hoặc không tồn tại.");
                var a=PayrollPreparationSql.Query<CaseRow>(db,"SELECT IDBATTHUONG,MANV,IDBANGCONGCT,MA_LOI,INPUT_HASH,MA_SU_VIEC_HASH,TRANG_THAI,INPUT_SNAPSHOT FROM TB_CHAMCONG_BATTHUONG WHERE IDBATTHUONG=:id FOR UPDATE",new OracleParameter("id",caseId)).Single();
                if(a.TRANG_THAI!="CHO_XU_LY" || !CanExclude(a.MA_LOI))throw new InvalidOperationException("Mục này phải xử lý bằng ca/giờ/đơn được duyệt, không thể bỏ qua.");
                var self=PayrollPreparationSql.Query<decimal?>(db,"SELECT EMPLOYEE_ID FROM TB_USER_EMPLOYEE_MAPPING WHERE USER_ID=:id",new OracleParameter("id",UserSession.CurrentUser.IDUSER)).FirstOrDefault();
                if(self==a.MANV)throw new UnauthorizedAccessException("Không tự xác minh công của chính mình.");
                var snapshots=PayrollPreparationSql.Query<string>(db,@"SELECT q.INPUT_SNAPSHOT FROM TB_BANGCONG_CHITIET b JOIN TB_CHAMCONG_KQ_NGAY q ON q.IDBANGCONGCT=b.IDBANGCONGCT AND q.IDLANTINH=b.LANTINH_ID_HIENHANH
                    JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH=q.IDLANTINH JOIN TB_KYCONG k ON k.MAKYCONG=b.MAKYCONG
                    WHERE b.IDBANGCONGCT=:id AND b.MAKYCONG=:p AND NVL(b.DU_DIEUKIEN_CHOT,0)=0 AND r.INPUT_REV=NVL(k.CONG_INPUT_REV,0)",new OracleParameter("id",a.IDBANGCONGCT),new OracleParameter("p",period));
                if(snapshots.Count!=1 || !SameEvidence(a.INPUT_SNAPSHOT,snapshots[0]))throw new InvalidOperationException("Nguồn công đã thay đổi hoặc mục cũ không khớp hiện trạng. Công bố lại công rồi đối chiếu mục đang hiệu lực.");
                var ordinal=PayrollPreparationSql.Query<decimal>(db,"SELECT NVL(MAX(THUTU),0)+1 FROM TB_CHAMCONG_BT_LICHSU WHERE IDBATTHUONG=:id",new OracleParameter("id",caseId)).Single();
                var snapshot=JsonConvert.SerializeObject(new {CaseId=caseId,InputHash=a.INPUT_HASH,Period=period,Action="EXCLUDE_CONFIRMED_UNPAID_INTERVAL",UserId=UserSession.CurrentUser.IDUSER,Reason=reason});
                db.Database.ExecuteSqlCommand(@"INSERT INTO TB_CHAMCONG_BT_LICHSU (IDBATTHUONG,THUTU,TU_TRANGTHAI,DEN_TRANGTHAI,INPUT_HASH_DUYET,QUYETDINH_SNAPSHOT,LY_DO,NGUOI_XULY)
                    VALUES (:id,:seq,'CHO_XU_LY','KHONG_TINH',:hash,:snapshot,:reason,:actor)",new OracleParameter("id",caseId),new OracleParameter("seq",ordinal),new OracleParameter("hash",a.INPUT_HASH),new OracleParameter("snapshot",OracleDbType.Clob){Value=snapshot},new OracleParameter("reason",OracleDbType.NVarchar2){Value=reason},new OracleParameter("actor",UserSession.CurrentUser.IDUSER));
                db.Database.ExecuteSqlCommand("UPDATE TB_CHAMCONG_BATTHUONG SET TRANG_THAI='KHONG_TINH' WHERE IDBATTHUONG=:id",new OracleParameter("id",caseId));
                db.Database.ExecuteSqlCommand("UPDATE TB_KYCONG SET CONG_INPUT_REV=NVL(CONG_INPUT_REV,0)+1 WHERE MAKYCONG=:p",new OracleParameter("p",period));tx.Commit();
            }
        }
        public static void RefreshChangedEvidence(OracleConnection conn,AttendanceAnomalyDto anomaly,long runId,long actor)
        {
            decimal id;string from,oldHash;
            using(var cmd=new OracleCommand("SELECT IDBATTHUONG,TRANG_THAI,INPUT_HASH FROM TB_CHAMCONG_BATTHUONG WHERE MA_SU_VIEC_HASH=:h",conn)) {
                cmd.BindByName=true;cmd.Parameters.Add("h",anomaly.MaSuViecHash);using(var r=cmd.ExecuteReader()) {if(!r.Read())return;id=Convert.ToDecimal(r[0]);from=r.GetString(1);oldHash=r.GetString(2);}
            }
            if(oldHash==anomaly.InputHash)return;
            var snapshot=JsonConvert.SerializeObject(new {Action="REOPEN_CHANGED_EVIDENCE",PreviousInputHash=oldHash,NewInputHash=anomaly.InputHash,RunId=runId});
            using(var cmd=new OracleCommand(@"INSERT INTO TB_CHAMCONG_BT_LICHSU (IDBATTHUONG,THUTU,TU_TRANGTHAI,DEN_TRANGTHAI,INPUT_HASH_DUYET,QUYETDINH_SNAPSHOT,LY_DO,NGUOI_XULY)
                SELECT :id,NVL(MAX(THUTU),0)+1,:old,'CHO_XU_LY',:hash,:snap,'Nguồn công thay đổi; quyết định cũ không còn áp dụng',:actor FROM TB_CHAMCONG_BT_LICHSU WHERE IDBATTHUONG=:id",conn)) {
                cmd.BindByName=true;cmd.Parameters.Add("id",id);cmd.Parameters.Add("old",from);cmd.Parameters.Add("hash",anomaly.InputHash);cmd.Parameters.Add("snap",OracleDbType.Clob).Value=snapshot;cmd.Parameters.Add("actor",actor);cmd.ExecuteNonQuery();
            }
            using(var cmd=new OracleCommand(@"UPDATE TB_CHAMCONG_BATTHUONG SET INPUT_HASH=:hash,INPUT_SNAPSHOT=:snap,TRANG_THAI='CHO_XU_LY',IDLANTINH_PHATHIEN=:run,IDLICH=:schedule,MO_TA=:description WHERE IDBATTHUONG=:id",conn)) {
                cmd.BindByName=true;cmd.Parameters.Add("hash",anomaly.InputHash);cmd.Parameters.Add("snap",OracleDbType.Clob).Value=anomaly.InputSnapshotJson;cmd.Parameters.Add("run",runId);cmd.Parameters.Add("schedule",(object)anomaly.IdLich??DBNull.Value);cmd.Parameters.Add("description",OracleDbType.NVarchar2).Value=anomaly.MoTa;cmd.Parameters.Add("id",id);cmd.ExecuteNonQuery();
            }
        }
        public static bool SameEvidence(string caseJson,string dayJson)
        {
            var a=JObject.Parse(caseJson);var b=JObject.Parse(dayJson);
            if((int?)a["EvidenceVersion"]!=2)return false;
            var schedule=b["Schedule"]?["IdLich"];
            if(!JToken.DeepEquals(a["MaNV"],b["MaNV"]) || !JToken.DeepEquals(a["ScheduleId"],schedule))return false;
            Func<JToken,JArray> sorted=x=>new JArray((x as JArray ?? new JArray()).OrderBy(v=>(long?)v["Mabc"]??0));
            return JToken.DeepEquals(sorted(a["Punches"]),sorted(b["Punches"]));
        }
    }
}
