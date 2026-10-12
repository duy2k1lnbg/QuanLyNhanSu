using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;

namespace Bu.CLASS_PAYROLL
{
    public sealed class PayrollPublicationReview
    {
        public int Period { get; set; }
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Published { get; set; }
        public int LegacyPending { get; set; }
        public bool PeriodLocked { get; set; }
        public bool HasPermission { get; set; }
        public bool CanPublish { get; set; }
        public bool CanRevoke { get; set; } // Compatibility only: publication cannot be revoked.
        public bool Frozen { get; set; }
        public string Stamp { get; set; }
        public List<string> BlockerCodes { get; set; } = new List<string>();
    }
    public sealed class PayrollPublicationCommand
    {
        public string ExpectedStamp { get; set; }
        public string Reason { get; set; }
        public bool ConfirmLegacy { get; set; }
    }
    public sealed class PayrollPublicationChange
    {
        public int Period { get; set; }
        public int Changed { get; set; }
    }
    public sealed class PayrollPublicationRow
    {
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal? IDCTY { get; set; }
        public decimal LEGACY { get; set; }
        public string STATUS { get; set; }
        public string PAYMENT { get; set; }
        public decimal? RUN_ID { get; set; }
        public string RUN_STATUS { get; set; }
        public decimal? RUN_ERRORS { get; set; }
        public DateTime? CALCULATED_AT { get; set; }
        public decimal? GROSS { get; set; }
        public decimal? NET { get; set; }
        public decimal INSURANCE { get; set; }
        public decimal UNION_FEE { get; set; }
        public decimal ADVANCE { get; set; }
        public decimal TAX { get; set; }
        public decimal OTHER { get; set; }
        public decimal REFUND { get; set; }
        public decimal SOURCES { get; set; }
        public decimal STALE { get; set; }
        public decimal? INPUT_AT_CALC { get; set; }
        public decimal? PUBLISH_AT_CALC { get; set; }
        public decimal CURRENT_INPUT { get; set; }
        public decimal CURRENT_PUBLISH { get; set; }
        public bool IsLegacy => LEGACY == 1 || STATUS == "LEGACY_READONLY";
        public bool IsPublished => PayrollPublicationHelper.IsStatusPublished(STATUS);
        public bool PaymentStarted => STATUS == "PAID" || STATUS == "DA_CHI_TRA" ||
            new[] {"PAID","DA_CHI_TRA","DANG_CHI_TRA"}.Contains(PAYMENT);
    }
    public sealed class PayrollPublicationService
    {
        public const string Capability = "F_CC_CONGBO_LUONG";
        private sealed class Actor { public decimal Id; public string Name; public string Channel; public decimal? Company; public bool Edit; }
        private static Actor Authorize(MyEntities db, decimal id, string channel, bool write)
        {
            var ch=AppChannels.Normalize(channel);
            if(ch!=AppChannels.Desktop && ch!=AppChannels.Web) throw new UnauthorizedAccessException("Công bố lương chỉ dành cho PC và Web quản trị.");
            var user=db.TB_SYS_USER.SingleOrDefault(u=>u.IDUSER==id);
            if(user==null || user.DISABLED==1 || user.ISGROUP==1) throw new UnauthorizedAccessException("Phiên quản trị không hợp lệ.");
            var rights=new ChannelPermissionResolver();
            if(!rights.CanExecuteAction(db,id,ch,"F_CC_BANGLUONG",ChannelAction.View,user.USERNAME)) throw new UnauthorizedAccessException("Bạn không có quyền xem bảng lương.");
            bool edit=rights.CanExecuteAction(db,id,ch,Capability,ChannelAction.Edit,user.USERNAME);
            if(write && !edit) throw new UnauthorizedAccessException("Cần quyền Công bố lương: Sửa trên nền tảng đang sử dụng.");
            decimal? company=null;
            if(!string.Equals(user.USERNAME,"ADMIN",StringComparison.OrdinalIgnoreCase)) {
                decimal value;
                if(!decimal.TryParse(user.MACTY,out value) || value<=0) throw new UnauthorizedAccessException("Tài khoản quản trị cần phạm vi công ty rõ ràng.");
                company=value;
            }
            return new Actor {Id=id,Name=user.USERNAME,Channel=ch,Company=company,Edit=edit};
        }
        private static OracleConnection Open(string connectionString)
        {
            var conn=new OracleConnection(connectionString);conn.Open();return conn;
        }
        private static void ValidatePeriod(int period)
        {
            if(!PayrollReadinessService.IsValidPeriod(period)) throw new ArgumentException("Kỳ công không hợp lệ.");
        }
        public PayrollPublicationReview Review(int period,decimal actorId,string channel)
        {
            ValidatePeriod(period);
            using(var db=new MyEntities()) {
                string connectionString=Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                var actor=Authorize(db,actorId,channel,false);
                using(var conn=Open(connectionString)) return ReadReview(conn,null,period,actor);
            }
        }
        public PayrollPublicationChange Publish(int period,decimal actorId,string channel,PayrollPublicationCommand command)
        { return Change(period,actorId,channel,command,false); }
        public PayrollPublicationChange Revoke(int period,decimal actorId,string channel,PayrollPublicationCommand command)
        { ValidatePeriod(period); using(var db=new MyEntities()) Authorize(db,actorId,channel,true); throw new BusinessException("PAYROLL_FROZEN",ErrorHelper.PayrollFrozenMessage); }
        private PayrollPublicationChange Change(int period,decimal actorId,string channel,PayrollPublicationCommand command,bool revoke)
        {
            ValidatePeriod(period);
            if(command==null || string.IsNullOrWhiteSpace(command.ExpectedStamp) || string.IsNullOrWhiteSpace(command.Reason) || command.Reason.Trim().Length<5 || command.Reason.Length>1000)
                throw new ArgumentException("Nhập căn cứ công bố từ 5 đến 1000 ký tự.");
            using(var db=new MyEntities()) {
                string connectionString=Bu.CLASS_CHAMCONG.AttendanceCalculationHelper.GetStoreConnectionString(db);
                var actor=Authorize(db,actorId,channel,true);
                using(var conn=Open(connectionString)) using(var tx=conn.BeginTransaction()) {
                    Scalar(conn,tx,"SELECT IDUSER FROM TB_SYS_USER WHERE IDUSER=:p0 FOR UPDATE",actorId);
                    actor=Authorize(db,actorId,channel,true);
                    var result=Apply(conn,tx,period,actor,command,revoke);tx.Commit();return result;
                }
            }
        }
        private static object Scalar(OracleConnection conn,OracleTransaction tx,string sql,params object[] values)
        {
            using(var cmd=new OracleCommand(sql,conn)){cmd.Transaction=tx;cmd.BindByName=true;for(int i=0;i<values.Length;i++)cmd.Parameters.Add("p"+i,values[i]??DBNull.Value);return cmd.ExecuteScalar();}
        }
        private static List<PayrollPublicationRow> ReadRows(OracleConnection conn,OracleTransaction tx,int period,Actor actor)
        {
            string payment=Convert.ToString(Scalar(conn,tx,"SELECT MIN(COLUMN_NAME) FROM ALL_TAB_COLUMNS WHERE OWNER='HR' AND TABLE_NAME='TB_BANGLUONG' AND COLUMN_NAME IN ('TRANGTHAI_CHITRA','TRANG_THAI_CHI_TRA')"));
            string paymentSql=string.IsNullOrEmpty(payment)?"CAST(NULL AS NVARCHAR2(30))":"b."+payment;
            string sql=@"SELECT b.IDBL,b.MANV,n.IDCTY,NVL(b.IS_LEGACY,0) LEGACY,
                NVL(b.TRANG_THAI,'DRAFT') STATUS,"+paymentSql+@" PAYMENT,b.RUN_ID,r.STATUS RUN_STATUS,r.ERROR_COUNT RUN_ERRORS,b.CALCULATED_AT,b.CONG_INPUT_REV_AT_CALC INPUT_AT_CALC,b.CONG_PUBLISH_REV_AT_CALC PUBLISH_AT_CALC,
                NVL(k.CONG_INPUT_REV,0) CURRENT_INPUT,NVL(k.CONG_PUBLISH_REV,0) CURRENT_PUBLISH,
                b.TONG_CONG GROSS,b.THUC_LINH NET,NVL(b.TIEN_BHXH_TRICH,0) INSURANCE,NVL(b.TIEN_CONG_DOAN,0) UNION_FEE,
                NVL(b.TIEN_TAMUNG,0) ADVANCE,NVL(b.THUE_TNCN,0) TAX,NVL(b.KHOAN_TRU_KHAC,0) OTHER,NVL(b.HOAN_THUE,0) REFUND,
                (SELECT COUNT(*) FROM TB_BANGLUONG_CT c JOIN TB_BANGLUONG_CT_SOURCE s ON s.IDBLCT=c.IDBLCT
                    WHERE c.IDBL=b.IDBL AND s.SOURCE_BCCT_ID IS NOT NULL) SOURCES,
                (SELECT COUNT(*) FROM TB_BANGLUONG_CT c JOIN TB_BANGLUONG_CT_SOURCE s ON s.IDBLCT=c.IDBLCT
                    LEFT JOIN TB_BANGCONG_CHITIET d ON d.IDBANGCONGCT=s.SOURCE_BCCT_ID
                    LEFT JOIN TB_CHAMCONG_LANTINH a ON a.IDLANTINH=d.LANTINH_ID_HIENHANH
                    WHERE c.IDBL=b.IDBL AND s.SOURCE_BCCT_ID IS NOT NULL AND
                    (d.IDBANGCONGCT IS NULL OR NVL(d.DU_DIEUKIEN_CHOT,0)<>1 OR a.IDLANTINH IS NULL OR
                     NVL(a.INPUT_REV,-1)<>NVL(k.CONG_INPUT_REV,0))) STALE
                FROM TB_BANGLUONG b JOIN TB_NHANVIEN n ON n.MANV=b.MANV JOIN TB_KYCONG k ON k.MAKYCONG=b.MAKYCONG
                LEFT JOIN TB_PAYROLL_CALCULATION_RUN r ON r.RUN_ID=b.RUN_ID
                WHERE b.MAKYCONG=:p0 AND (:p1 IS NULL OR n.IDCTY=:p1) ORDER BY b.IDBL";
            var rows=new List<PayrollPublicationRow>();
            using(var cmd=new OracleCommand(sql,conn)){
                cmd.Transaction=tx;cmd.BindByName=true;cmd.Parameters.Add("p0",period);cmd.Parameters.Add("p1",actor.Company.HasValue?(object)actor.Company.Value:DBNull.Value);
                using(var reader=cmd.ExecuteReader())while(reader.Read()){
                    var row=new PayrollPublicationRow();
                    foreach(var property in typeof(PayrollPublicationRow).GetProperties().Where(p=>p.CanWrite)){
                        object value=reader[property.Name];if(value==DBNull.Value)continue;
                        var type=Nullable.GetUnderlyingType(property.PropertyType)??property.PropertyType;
                        property.SetValue(row,Convert.ChangeType(value,type),null);
                    }
                    row.STATUS=row.STATUS.Trim().ToUpperInvariant();row.PAYMENT=row.PAYMENT?.Trim().ToUpperInvariant();rows.Add(row);
                }
            }return rows;
        }
        private static PayrollPublicationReview BuildReview(int period,bool locked,bool permission,List<PayrollPublicationRow> rows)
        {
            var pending=rows.Where(r=>!r.IsPublished).ToList();
            var review=new PayrollPublicationReview {Period=period,PeriodLocked=locked,HasPermission=permission,Total=rows.Count,
                Published=rows.Count(r=>r.IsPublished),Pending=pending.Count,LegacyPending=pending.Count(r=>r.IsLegacy)};
            if(rows.Count==0)review.BlockerCodes.Add("EMPTY_PAYROLL");
            if(pending.Any(r=>!r.IsLegacy) && !locked)review.BlockerCodes.Add("LOCK_PERIOD");
            if(pending.Any(r=>!r.IsLegacy && (r.STATUS!="CALCULATED" || !r.RUN_ID.HasValue || r.RUN_STATUS!="SUCCESS" || r.RUN_ERRORS!=0 || !r.CALCULATED_AT.HasValue || r.SOURCES<=0 || !r.INPUT_AT_CALC.HasValue || !r.PUBLISH_AT_CALC.HasValue)))review.BlockerCodes.Add("CALCULATION_REQUIRED");
            if(pending.Any(r=>!r.IsLegacy && (r.STALE>0 || r.STATUS=="CAN_TINH_LAI" || (r.INPUT_AT_CALC.HasValue && r.INPUT_AT_CALC!=r.CURRENT_INPUT) || (r.PUBLISH_AT_CALC.HasValue && r.PUBLISH_AT_CALC!=r.CURRENT_PUBLISH))))review.BlockerCodes.Add("INPUT_CHANGED");
            if(pending.Any(r=>!r.GROSS.HasValue || !r.NET.HasValue || r.GROSS<0 || (!r.IsLegacy && Math.Abs(r.NET.Value-(r.GROSS.Value-r.INSURANCE-r.UNION_FEE-r.ADVANCE-r.TAX-r.OTHER+r.REFUND))>1)))review.BlockerCodes.Add("INVALID_TOTALS");
            if(pending.Any(r=>r.IsLegacy && (r.LEGACY!=1 || r.STATUS!="LEGACY_READONLY")))review.BlockerCodes.Add("UNSUPPORTED_STATE");
            review.CanPublish=permission && pending.Count>0 && review.BlockerCodes.Count==0;
            review.CanRevoke=false;
            using(var hash=SHA256.Create())review.Stamp=Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {period,locked,rows}))));
            return review;
        }
        private static PayrollPublicationReview ReadReview(OracleConnection conn,OracleTransaction tx,int period,Actor actor)
        {
            var locked=Scalar(conn,tx,"SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0",period);
            if(locked==null)throw new InvalidOperationException("Kỳ công chưa được tạo.");
            var review=BuildReview(period,Convert.ToInt32(locked)==1,actor.Edit,ReadRows(conn,tx,period,actor));
            review.Frozen=Convert.ToInt32(Scalar(conn,tx,"SELECT LUONG_CONGBO_LOCKED FROM TB_KYCONG WHERE MAKYCONG=:p0",period))==1;
            if(review.Frozen && review.Pending>0) {review.CanPublish=false;review.BlockerCodes.Add("PAYROLL_FROZEN");}
            // The attendance period and its revisions are shared across companies.
            if(actor.Company.HasValue && Convert.ToInt32(Scalar(conn,tx,"SELECT COUNT(*) FROM TB_BANGLUONG WHERE MAKYCONG=:p0",period))!=review.Total) {
                review.CanPublish=false;review.BlockerCodes.Add("GLOBAL_PERIOD_SCOPE");
            }
            review.Stamp=review.Stamp+":"+(review.Frozen?"1":"0");
            return review;
        }
        private static PayrollPublicationChange Apply(OracleConnection conn,OracleTransaction tx,int period,Actor actor,PayrollPublicationCommand command,bool revoke)
        {
            if(revoke)throw new BusinessException("PAYROLL_FROZEN",ErrorHelper.PayrollFrozenMessage);
            Scalar(conn,tx,"SELECT MAKYCONG FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",period);
            using(var lockRows=new OracleCommand("SELECT IDBL FROM TB_BANGLUONG WHERE MAKYCONG=:p0 FOR UPDATE",conn)){
                lockRows.Transaction=tx;lockRows.Parameters.Add("p0",period);using(var reader=lockRows.ExecuteReader())while(reader.Read()){}
            }
            var review=ReadReview(conn,tx,period,actor);
            if(!revoke && review.Total>0 && review.Pending==0)return new PayrollPublicationChange{Period=period,Changed=0};
            if(review.Stamp!=command.ExpectedStamp)throw new InvalidOperationException("Dữ liệu đã thay đổi. Kiểm tra lại trước khi công bố.");
            if(revoke ? !review.CanRevoke : !review.CanPublish)throw new InvalidOperationException("Chưa đủ điều kiện thực hiện. Kiểm tra hướng dẫn trong Công bố lương.");
            if(!revoke && review.LegacyPending>0 && !command.ConfirmLegacy)throw new InvalidOperationException("Cần xác nhận đã đối chiếu hồ sơ lương lịch sử.");
            var rows=ReadRows(conn,tx,period,actor).Where(r=>!r.IsPublished).ToList();
            using(var cmd=new OracleCommand(@"UPDATE TB_BANGLUONG b SET TRANG_THAI='PUBLISHED'
                WHERE b.MAKYCONG=:p0 AND EXISTS(SELECT 1 FROM TB_NHANVIEN n WHERE n.MANV=b.MANV AND (:p1 IS NULL OR n.IDCTY=:p1))",conn)) {
                cmd.Transaction=tx;cmd.BindByName=true;cmd.Parameters.Add("p0",period);cmd.Parameters.Add("p1",actor.Company.HasValue?(object)actor.Company.Value:DBNull.Value);
                if(cmd.ExecuteNonQuery()!=rows.Count)throw new InvalidOperationException("Phiếu lương đã thay đổi. Tải lại dữ liệu.");
            }
            foreach(var row in rows){
                string next="PUBLISHED",action="CONG_BO_BANGLUONG";
                using(var audit=new OracleCommand(@"INSERT INTO TB_SYS_LOG
                    (MANV_THUCHIEN,TEN_THUCHIEN,HANHDONG,TEN_BANG,ID_BAN_GHI,DU_LIEU_MOI,THOIGIAN,MODULE_NAME,TEN_MAY_TINH)
                    VALUES (:p_actor,:p_name,:p_action,'TB_BANGLUONG',:p_id,:p_data,SYSTIMESTAMP,'PAYROLL_PUBLICATION',:p_channel)",conn)){
                    audit.Transaction=tx;audit.BindByName=true;audit.Parameters.Add("p_actor",actor.Id);audit.Parameters.Add("p_name",actor.Name);
                    audit.Parameters.Add("p_action",action);audit.Parameters.Add("p_id",row.IDBL.ToString());
                    audit.Parameters.Add("p_data",JsonConvert.SerializeObject(new {period,oldStatus=row.STATUS,newStatus=next,reason=command.Reason.Trim(),channel=actor.Channel,legacyAcknowledged=command.ConfirmLegacy}));
                    audit.Parameters.Add("p_channel",actor.Channel);audit.ExecuteNonQuery();
                }
                NotificationService.CreateNotification(conn,tx,"Phiếu lương đã được công bố",
                    $"Phiếu lương kỳ {period} đã được công bố. Mở Bảng lương để xem chi tiết. Công bố không đồng nghĩa đã chi trả.",
                    "PAYROLL",actor.Name,"PERSONAL",(long)row.MANV,null,null,"PAYROLL_PUBLISHED",row.IDBL,null);
            }
            return new PayrollPublicationChange{Period=period,Changed=rows.Count};
        }
    }
}
