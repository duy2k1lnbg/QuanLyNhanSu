using Bu.CLASS_SYSTEM;
using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Bu.CLASS_PAYROLL
{
    public sealed class PayrollDataSection
    {
        public string Table { get; set; }
        public string Title { get; set; }
        public string Sequence { get; set; }
        public string Fields { get; set; }
        public string Capability { get; set; } = "F_CC_BANGLUONG";
        public string StartField { get; set; }
        public string EndField { get; set; }
        public string ReferenceField { get; set; }
        public bool IsRequest => Table.StartsWith("TB_YEUCAU_",StringComparison.Ordinal);
        public bool IsAnomaly => Table=="TB_CHAMCONG_BATTHUONG";
        public bool IsAttendanceConfiguration => new[]{"TB_CA_PHIENBAN","TB_CA_KHUNGGIO","TB_QUYDINH_CHAMCONG","TB_HESO_TANGCA"}.Contains(Table);
        public bool IsSchedule => Table=="TB_LICH_LAMVIEC";
        public override string ToString()=>Title;
    }
    public sealed class PayrollDataField { public string Name { get; set; } public string Type { get; set; } public bool Required { get; set; } public int Length { get; set; } }
    public sealed class PayrollDataPage { public PayrollDataSection Section { get; set; } public List<PayrollDataField> Fields { get; set; } public DataTable Rows { get; set; } }
    public sealed class PayrollProfileUsage
    {
        public decimal MAKYCONG { get; set; }
        public decimal LOCKED { get; set; }
        public decimal LEGACY { get; set; }
        public decimal PAYMENT_BLOCKED { get; set; }
        public string STATUS { get; set; }
    }
    public sealed class PayrollPreparationSaveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ErrorCode { get; set; }
        public string TargetField { get; set; }
        public int[] RecalculatePeriods { get; set; } = new int[0];
    }
    public sealed class PayrollPreparationDataService
    {
        private static PayrollDataSection Section(string table,string title,string sequence,string fields,string from=null,string to=null,string reference=null)=>new PayrollDataSection {Table=table,Title=title,Sequence=sequence,Fields=fields,StartField=from,EndField=to,ReferenceField=reference};
        // Table/column names are compile-time allowlisted. Values always use bound parameters.
        public static readonly PayrollDataSection[] Sections = {
            Section("TB_CHINH_SACH_LUONG","Chính sách lương","SEQ_CHINH_SACH_LUONG","MA_CHINH_SACH,TEN_CHINH_SACH,NGAY_HIEU_LUC,NGAY_HET_HIEU_LUC,SO_CONG_CHUAN_THANG,SO_GIO_CHUAN_NGAY,HE_SO_LAM_DEM,TRANG_THAI","NGAY_HIEU_LUC","NGAY_HET_HIEU_LUC","POLICY_LUONG_ID"),
            Section("TB_CHINH_SACH_BHXH","Chính sách BHXH","SEQ_CHINH_SACH_BHXH","MA_CHINH_SACH,TEN_CHINH_SACH,NGAY_HIEU_LUC,NGAY_HET_HIEU_LUC,MUC_THAM_CHIEU,TY_LE_BHXH_NLD,TY_LE_BHYT_NLD,TY_LE_BHTN_NLD,TY_LE_BHXH_NSDLD,TY_LE_BHYT_NSDLD,TY_LE_BHTN_NSDLD,TY_LE_TNLD_BNN_NSDLD,TY_LE_TNLD_BNN_UU_DAI,AP_DUNG_TRAN_BHXH_BHYT,AP_DUNG_TRAN_BHTN,TRANG_THAI","NGAY_HIEU_LUC","NGAY_HET_HIEU_LUC","POLICY_BHXH_ID"),
            Section("TB_CHINH_SACH_BHXH_VUNG","Mức lương tối thiểu vùng","SEQ_CHINH_SACH_BHXH_VUNG","POLICY_BHXH_ID,VUNG_LUONG,LUONG_TOI_THIEU_THANG,LUONG_TOI_THIEU_GIO,HE_SO_SAN_DOANH_NGHIEP"),
            Section("TB_CHINH_SACH_CONG_DOAN","Chính sách công đoàn","SEQ_CHINH_SACH_CONG_DOAN","MA_CHINH_SACH,TEN_CHINH_SACH,NGAY_HIEU_LUC,NGAY_HET_HIEU_LUC,TY_LE_DOAN_PHI_NLD,CAP_PERCENT_STATUTORY_BASE_SALARY,KINH_PHI_CONG_DOAN_NSDLD,TRANG_THAI","NGAY_HIEU_LUC","NGAY_HET_HIEU_LUC","POLICY_CONGDOAN_ID"),
            Section("TB_THUE_TNCN_CHINH_SACH","Chính sách giảm trừ thuế","SEQ_THUE_TNCN_CHINH_SACH","MA_CHINH_SACH,TEN_CHINH_SACH,TAX_YEAR,NGAY_HIEU_LUC,NGAY_HET_HIEU_LUC,GIAM_TRU_BAN_THAN_THANG,GIAM_TRU_PHU_THUOC_THANG,GIAM_TRU_BAN_THAN_NAM,GIAM_TRU_PHU_THUOC_NAM,TRANG_THAI","NGAY_HIEU_LUC","NGAY_HET_HIEU_LUC","POLICY_THUE_ID"),
            Section("TB_THUE_TNCN_BAC","Bậc thuế TNCN","SEQ_THUE_TNCN_BAC","POLICY_THUE_ID,TAX_YEAR,PERIOD_TYPE,BAC_THUE,CAN_DUOI,CAN_TREN,THUE_SUAT"),
            Section("TB_NHANVIEN_BAOHIEM_THAM_GIA","Hồ sơ bảo hiểm","SEQ_NV_BAOHIEM_TG","MANV,VUNG_LUONG,THAM_GIA_BHXH,THAM_GIA_BHYT,THAM_GIA_BHTN,THAM_GIA_TNLD_BNN,HUONG_TY_LE_TNLD_UU_DAI,LUONG_DONG_BHXH_RIENG,NGAY_BAT_DAU,NGAY_KET_THUC,TRANG_THAI","NGAY_BAT_DAU","NGAY_KET_THUC","PROFILE_BH_ID"),
            Section("TB_NHANVIEN_CONG_DOAN_THAM_GIA","Hồ sơ công đoàn","SEQ_NV_CONGDOAN_TG","MANV,LA_DOAN_VIEN,NGAY_GIA_NHAP,NGAY_KET_THUC,TRANG_THAI","NGAY_GIA_NHAP","NGAY_KET_THUC","PROFILE_CD_ID"),
            Section("TB_NHANVIEN_THUE","Hồ sơ thuế","SEQ_NV_THUE","MANV,MA_SO_THUE,LOAI_DINH_DANH,SO_HO_CHIEU,IS_CU_TRU,CAN_CU_CU_TRU,CO_UY_QUYEN_QUYET_TOAN,TRANG_THAI_HO_SO,NGAY_HIEU_LUC,NGAY_HET_HIEU_LUC,TRANG_THAI","NGAY_HIEU_LUC","NGAY_HET_HIEU_LUC","PROFILE_THUE_ID"),
            Section("TB_NGUOI_PHU_THUOC","Người phụ thuộc","SEQ_NGUOI_PHU_THUOC","MANV,HO_TEN,MOI_QUAN_HE,NGAY_SINH,LOAI_GIAY_TO,SO_GIAY_TO,MA_DINH_DANH_CA_NHAN,CCCD,MA_SO_THUE_NPT,CAN_CU_GIAM_TRU,TRANG_THAI_DANG_KY,TINH_TRANG_HO_SO_CHUNG_MINH,THANG_BAT_DAU_GIAM_TRU,THANG_KET_THUC_GIAM_TRU,TRANG_THAI"),
            Section("TB_DANG_KY_GIAM_TRU_NPT","Đăng ký giảm trừ NPT","SEQ_DANG_KY_GIAM_TRU_NPT","ID_NPT,MANV,THANG_BAT_DAU,THANG_KET_THUC,TRANG_THAI,TRANG_THAI_DANG_KY,LY_DO_CHUYEN"),
            new PayrollDataSection {Table="TB_LICH_LAMVIEC",Title="Phân ca / ngày nghỉ",Capability="F_CC_BANGCONG",Fields="MANV,NGAY,IDCAPHIENBAN,IDQUYDINH,BATDAU_KEHOACH,KETTHUC_KEHOACH,TRANG_THAI_PHAN_CONG,LOAI_NGAY,LY_DO"},
            new PayrollDataSection {Table="TB_YEUCAU_NGHIPHEP",Title="Đơn nghỉ / xác nhận vắng",Capability="F_NV_PHEDUYET",Fields="MANV,LOAIPHEP,TUNGAY,DENNGAY,SONGAY,LYDO,BATDAU_NGHI,KETTHUC_NGHI,GIAY_NGHI,NGUON_CHI_TRA,LOAI_HUONG_CONG"},
            new PayrollDataSection {Table="TB_YEUCAU_DIEUCHINHCONG",Title="Đơn điều chỉnh giờ công",Capability="F_NV_PHEDUYET",Fields="MANV,NGAY,LOAIDIEUCHINH,LYDO,THOIDIEM_VAO_MOI,THOIDIEM_RA_MOI"},
            new PayrollDataSection {Table="TB_YEUCAU_TANGCA",Title="Đơn tăng ca",Capability="F_NV_PHEDUYET",Fields="MANV,NGAY,GIOTANGCA,IDCA,LYDO,BATDAU_DANGKY,KETTHUC_DANGKY"}
            ,new PayrollDataSection {Table="TB_CHAMCONG_BATTHUONG",Title="Xác minh bất thường công",Capability="F_NV_PHEDUYET",Fields="MANV,NGAY,MA_LOI,BATDAU_LUC,KETTHUC_LUC,MO_TA,TRANG_THAI"}
            ,new PayrollDataSection {Table="TB_CA_PHIENBAN",Title="Phiên bản ca làm việc",Capability="F_CC_BANGCONG",Fields="IDLOAICA,SO_PHIENBAN,TEN_PHIENBAN,TU_NGAY,DEN_NGAY,TONG_GIAY_CHUAN,CONG_QUY_DOI,TRANG_THAI"}
            ,new PayrollDataSection {Table="TB_CA_KHUNGGIO",Title="Khung giờ trong ca",Capability="F_CC_BANGCONG",Fields="IDCAPHIENBAN,STT,BATDAU_PHUT,KETTHUC_PHUT,LOAI_KHUNGGIO,BAT_BUOC_QUET_THE"}
            ,new PayrollDataSection {Table="TB_QUYDINH_CHAMCONG",Title="Quy định tính công",Capability="F_CC_BANGCONG",Fields="MA_QUYDINH,SO_PHIENBAN,TEN_QUYDINH,TU_NGAY,DEN_NGAY,GIAY_MIEN_VIPHAM_MUON,GIAY_MIEN_VIPHAM_SOM,GIAY_DUNG_SAI_HUONG_CONG,CACH_HUONG_DUNG_SAI,CACH_DEM_MUON,CUA_SO_VAO_TRUOC_GIAY,CUA_SO_RA_SAU_GIAY,GIOI_HAN_GHEP_GIAY,MUI_GIO,DEM_BATDAU_PHUT,DEM_KETTHUC_PHUT,KIEU_LAM_TRON,BUOC_LAM_TRON_GIAY,TRANG_THAI"}
            ,new PayrollDataSection {Table="TB_HESO_TANGCA",Title="Hệ số trả lương tăng ca",Capability="F_CC_BANGLUONG",Fields="TEN_QUYDINH,LOAICONG,LOAICA,GIO_BATDAU,GIO_KETTHUC,HESO_CHINHTHUC,HESO_THUVIEC,TU_NGAY,DEN_NGAY,SO_PHIENBAN,LA_BAN_DEM,CO_OT_BAN_NGAY_TRUOC,TRANG_THAI"}
        };
        public static PayrollDataSection Find(string table)=>Sections.Single(s=>s.Table==table);
        public static string Key(PayrollDataSection s)=>s.IsSchedule ? "IDLICH" : s.IsAnomaly ? "IDBATTHUONG" : s.Table=="TB_CA_PHIENBAN" ? "IDCAPHIENBAN" : s.Table=="TB_CA_KHUNGGIO" ? "IDKHUNGGIO" : s.Table=="TB_QUYDINH_CHAMCONG" ? "IDQUYDINH" : "ID";
        public static bool IsFlag(string name)=>name.StartsWith("THAM_GIA_") || name.StartsWith("AP_DUNG_TRAN_") || new[] {"LA_DOAN_VIEN","IS_CU_TRU","CO_UY_QUYEN_QUYET_TOAN","HUONG_TY_LE_TNLD_UU_DAI","LA_BAN_DEM","CO_OT_BAN_NGAY_TRUOC","BAT_BUOC_QUET_THE"}.Contains(name);
        public static void Demand(string table,bool add=false,bool edit=false)
        {
            var s=Find(table);
            if(!UserSession.IsLoggedIn || !UserSession.CanView(s.Capability) || (add && !UserSession.CanAdd(s.Capability)) || (edit && !UserSession.CanEdit(s.Capability)))
                throw new UnauthorizedAccessException("Chưa được cấp quyền "+s.Title+". Hãy yêu cầu quản trị viên cấp quyền xem/thêm/sửa tương ứng.");
        }
        public PayrollDataPage Load(string table,int period,decimal? employee=null)
        {
            Demand(table); var s=Find(table);
            using(var db=new MyEntities()) {
                var conn=(OracleConnection)db.Database.Connection; conn.Open();
                var fields=new List<PayrollDataField>();
                using(var cmd=new OracleCommand("SELECT COLUMN_NAME,DATA_TYPE,NULLABLE,CHAR_LENGTH FROM ALL_TAB_COLUMNS WHERE OWNER=SYS_CONTEXT('USERENV','CURRENT_SCHEMA') AND TABLE_NAME=:t",conn)) {
                    cmd.BindByName=true; cmd.Parameters.Add("t",table);
                    using(var r=cmd.ExecuteReader()) while(r.Read()) fields.Add(new PayrollDataField {Name=r.GetString(0),Type=r.GetString(1),Required=r.GetString(2)=="N",Length=Convert.ToInt32(r[3])});
                }
                var names=s.Fields.Split(','); fields=names.Select(n=>fields.Single(f=>f.Name==n)).ToList();
                string filter=""; var parameters=new List<OracleParameter>();
                if(s.IsSchedule || s.IsRequest || s.IsAnomaly) {
                    var date=s.Table=="TB_YEUCAU_NGHIPHEP" ? "TUNGAY" : "NGAY";
                    filter=$" WHERE {date}<:last AND "+(s.Table=="TB_YEUCAU_NGHIPHEP" ? "DENNGAY>=:first" : date+">=:first");
                    parameters.Add(new OracleParameter("last",new DateTime(period/100,period%100,1).AddMonths(1))); parameters.Add(new OracleParameter("first",new DateTime(period/100,period%100,1)));
                    if(s.IsAnomaly) filter+=" AND TRANG_THAI='CHO_XU_LY' AND EXISTS (SELECT 1 FROM TB_BANGCONG_CHITIET b WHERE b.IDBANGCONGCT=TB_CHAMCONG_BATTHUONG.IDBANGCONGCT AND NVL(b.DU_DIEUKIEN_CHOT,0)=0)";
                    if(s.IsSchedule) filter+=" AND TRANG_THAI='PUBLISHED'";
                }
                if(employee.HasValue && names.Contains("MANV")) {filter+=(filter.Length==0?" WHERE ":" AND ")+"MANV=:employee"; parameters.Add(new OracleParameter("employee",employee.Value));}
                var rows=new DataTable();
                using(var cmd=new OracleCommand($"SELECT {Key(s)},{s.Fields}"+(s.IsRequest?",TRANGTHAI":"")+$" FROM {s.Table}{filter} ORDER BY {Key(s)} DESC",conn)) {
                    cmd.BindByName=true; cmd.Parameters.AddRange(parameters.ToArray()); using(var a=new OracleDataAdapter(cmd)) a.Fill(rows);
                }
                return new PayrollDataPage {Section=s,Fields=fields,Rows=rows};
            }
        }
        public DataTable Lookup(string field)
        {
            // Only fixed lookup queries, never supplied SQL.
            string sql=field=="MANV" ? "SELECT MANV ID,EMPLOYEE_CODE||' - '||HOTEN LABEL FROM TB_NHANVIEN WHERE DELETED_DATE IS NULL ORDER BY MANV" :
                field=="ID_NPT" ? "SELECT ID,HO_TEN||' (ID '||ID||')' LABEL FROM TB_NGUOI_PHU_THUOC ORDER BY ID" :
                field=="IDCAPHIENBAN" ? "SELECT IDCAPHIENBAN ID,TEN_PHIENBAN LABEL FROM TB_CA_PHIENBAN ORDER BY IDCAPHIENBAN" :
                field=="IDQUYDINH" ? "SELECT IDQUYDINH ID,MA_QUYDINH LABEL FROM TB_QUYDINH_CHAMCONG WHERE TRANG_THAI='PUBLISHED' ORDER BY IDQUYDINH" :
                field=="IDCA" || field=="IDLOAICA" ? "SELECT IDLOAICA ID,TENLOAICA LABEL FROM TB_LOAICA ORDER BY IDLOAICA" :
                field=="POLICY_BHXH_ID" ? "SELECT ID,TEN_CHINH_SACH LABEL FROM TB_CHINH_SACH_BHXH ORDER BY ID" :
                field=="POLICY_THUE_ID" ? "SELECT ID,TEN_CHINH_SACH LABEL FROM TB_THUE_TNCN_CHINH_SACH ORDER BY ID" : null;
            if(sql==null) return null;
            using(var db=new MyEntities()) {var c=(OracleConnection)db.Database.Connection; c.Open(); var t=new DataTable(); using(var a=new OracleDataAdapter(sql,c)) a.Fill(t); return t;}
        }
        public static void Validate(PayrollDataSection s,IDictionary<string,object> values)
        {
            var allowed=s.Fields.Split(',');
            if(values.Keys.Any(k=>!allowed.Contains(k)) || allowed.Any(k=>!values.ContainsKey(k))) throw new ArgumentException("Các trường nhập không khớp loại dữ liệu được phép.");
            foreach(var pair in values) {
                if(pair.Value==null || pair.Value==DBNull.Value) continue;
                if(IsFlag(pair.Key) && Convert.ToDecimal(pair.Value)!=0 && Convert.ToDecimal(pair.Value)!=1) throw new ArgumentException("Lựa chọn Có/Không không hợp lệ.");
                if(pair.Key.StartsWith("TY_LE_") || pair.Key=="THUE_SUAT" || pair.Key=="KINH_PHI_CONG_DOAN_NSDLD" || pair.Key=="CAP_PERCENT_STATUTORY_BASE_SALARY")
                    if(Convert.ToDecimal(pair.Value)<0 || Convert.ToDecimal(pair.Value)>1) throw new ArgumentException("Tỷ lệ phải nằm từ 0 đến 1, ví dụ 0,08 tương ứng 8%.");
                if(pair.Key=="VUNG_LUONG" && (Convert.ToDecimal(pair.Value)<1 || Convert.ToDecimal(pair.Value)>4)) throw new ArgumentException("Vùng lương phải từ I đến IV.");
                if(pair.Key=="TRANG_THAI" && !s.IsAttendanceConfiguration && !new[]{"ACTIVE","INACTIVE"}.Contains(Convert.ToString(pair.Value))) throw new ArgumentException("Trạng thái hồ sơ/chính sách không hợp lệ.");
                if(pair.Key.StartsWith("THANG_") && !PayrollReadinessService.IsValidPeriod(Convert.ToInt32(pair.Value))) throw new ArgumentException("Tháng giảm trừ phải theo dạng YYYYMM.");
            }
            if(s.StartField!=null && values[s.StartField]!=DBNull.Value && values[s.EndField]!=DBNull.Value && Convert.ToDateTime(values[s.EndField])<Convert.ToDateTime(values[s.StartField])) throw new ArgumentException("Ngày kết thúc không được trước ngày bắt đầu.");
            if(s.Table=="TB_THUE_TNCN_BAC") {
                if(!new[]{"MONTH","YEAR"}.Contains(Convert.ToString(values["PERIOD_TYPE"]))) throw new ArgumentException("Chọn biểu thuế tháng hoặc năm.");
                if(Convert.ToDecimal(values["CAN_DUOI"])<0 || (values["CAN_TREN"]!=DBNull.Value && Convert.ToDecimal(values["CAN_TREN"])<=Convert.ToDecimal(values["CAN_DUOI"]))) throw new ArgumentException("Cận thuế không hợp lệ.");
            }
            if(s.Table=="TB_NHANVIEN_THUE") {
                if(values.ContainsKey("IS_CU_TRU") && values.ContainsKey("CO_UY_QUYEN_QUYET_TOAN") && values["IS_CU_TRU"]!=DBNull.Value && values["CO_UY_QUYEN_QUYET_TOAN"]!=DBNull.Value) {
                    if(Convert.ToDecimal(values["IS_CU_TRU"])==0 && Convert.ToDecimal(values["CO_UY_QUYEN_QUYET_TOAN"])==1)
                        throw new ArgumentException("Cá nhân không cư trú không được ủy quyền quyết toán thuế.", "CO_UY_QUYEN_QUYET_TOAN");
                }
                if(values.ContainsKey("TRANG_THAI_HO_SO") && Convert.ToString(values["TRANG_THAI_HO_SO"])=="DA_XAC_MINH") {
                    bool hasId = (values.ContainsKey("MA_SO_THUE") && values["MA_SO_THUE"]!=DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(values["MA_SO_THUE"])))
                              || (values.ContainsKey("SO_HO_CHIEU") && values["SO_HO_CHIEU"]!=DBNull.Value && !string.IsNullOrWhiteSpace(Convert.ToString(values["SO_HO_CHIEU"])));
                    if(!hasId) throw new ArgumentException("Hồ sơ đã xác minh phải có Mã số thuế hoặc Số hộ chiếu. Hãy bổ sung định danh trước khi lưu trạng thái Đã xác minh.", values.ContainsKey("LOAI_DINH_DANH") && Convert.ToString(values["LOAI_DINH_DANH"])=="HO_CHIEU" ? "SO_HO_CHIEU" : "MA_SO_THUE");
                }
            }
            if(s.Table=="TB_NGUOI_PHU_THUOC") {
                if(values.ContainsKey("NGAY_SINH") && values["NGAY_SINH"]!=DBNull.Value && Convert.ToDateTime(values["NGAY_SINH"]).Date>DateTime.Today)
                    throw new ArgumentException("Ngày sinh người phụ thuộc không được ở tương lai.");
                if(values.ContainsKey("THANG_BAT_DAU_GIAM_TRU") && values.ContainsKey("THANG_KET_THUC_GIAM_TRU") && values["THANG_BAT_DAU_GIAM_TRU"]!=DBNull.Value && values["THANG_KET_THUC_GIAM_TRU"]!=DBNull.Value) {
                    if(Convert.ToInt32(values["THANG_KET_THUC_GIAM_TRU"])<Convert.ToInt32(values["THANG_BAT_DAU_GIAM_TRU"]))
                        throw new ArgumentException("Tháng kết thúc giảm trừ không được trước tháng bắt đầu.");
                }
            }
            if(s.Table=="TB_DANG_KY_GIAM_TRU_NPT") {
                if(values.ContainsKey("THANG_BAT_DAU") && values.ContainsKey("THANG_KET_THUC") && values["THANG_BAT_DAU"]!=DBNull.Value && values["THANG_KET_THUC"]!=DBNull.Value) {
                    if(Convert.ToInt32(values["THANG_KET_THUC"])<Convert.ToInt32(values["THANG_BAT_DAU"]))
                        throw new ArgumentException("Tháng kết thúc giảm trừ không được trước tháng bắt đầu.");
                }
            }
            if(s.IsRequest && string.IsNullOrWhiteSpace(Convert.ToString(values["LYDO"]))) throw new ArgumentException("Phải nhập lý do và căn cứ yêu cầu.");
            if(s.IsSchedule && string.IsNullOrWhiteSpace(Convert.ToString(values["LY_DO"]))) throw new ArgumentException("Phải nhập lý do thay đổi phân ca.");
        }
        public void Save(string table,IDictionary<string,object> values,IDictionary<string,object> original,int period,string reason,bool validateOnly=false)
        {
            SaveCore(table,values,original,period,reason,validateOnly);
        }
        // Desktop consumes expected business failures as results, inside the worker's user-code frame.
        public PayrollPreparationSaveResult TrySave(string table,IDictionary<string,object> values,IDictionary<string,object> original,int period,string reason,bool validateOnly=false)
        {
            try { return new PayrollPreparationSaveResult { Success=true, RecalculatePeriods=SaveCore(table,values,original,period,reason,validateOnly) }; }
            catch (BusinessException e) { return new PayrollPreparationSaveResult { ErrorCode=e.ErrorCode, Message=e.Message, TargetField=e.TargetField }; }
            catch (ArgumentException e) { return new PayrollPreparationSaveResult { ErrorCode="INVALID_INPUT", Message=e.Message.Split(new[]{Environment.NewLine},StringSplitOptions.None)[0], TargetField=e.ParamName }; }
            catch (UnauthorizedAccessException e) { return new PayrollPreparationSaveResult { ErrorCode="PERMISSION_DENIED", Message=e.Message }; }
        }
        private int[] SaveCore(string table,IDictionary<string,object> values,IDictionary<string,object> original,int period,string reason,bool validateOnly)
        {
            var s=Find(table); if(s.IsAnomaly) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Bất thường chỉ được xử lý bằng quyết định xác minh hoặc sửa nguồn công."); Demand(table,original==null,original!=null); Validate(s,values); if(s.IsSchedule) ValidateSchedule(values,period);
            if(string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Nhập căn cứ/lý do lưu dữ liệu.");
            using(var db=new MyEntities()) using(var tx=db.Database.BeginTransaction()) {
                var conn=(OracleConnection)db.Database.Connection;
                var recalculate = new int[0];
                var key=Key(s); decimal? id=original==null ? (decimal?)null : Convert.ToDecimal(original[key]);
                // Serialize editor writes. Request/schedule writes lock the payroll period too.
                if(s.IsSchedule || s.IsRequest) CheckOpenPeriod(db,period);
                else {
                    // Same lock order as payroll commit, before configuration records, prevents a concurrent calculation
                    // from committing against a policy/profile record being edited underneath it.
                    PayrollPreparationSql.Query<decimal>(db,"SELECT MAKYCONG FROM TB_KYCONG ORDER BY MAKYCONG FOR UPDATE");
                    db.Database.ExecuteSqlCommand($"LOCK TABLE {table} IN SHARE ROW EXCLUSIVE MODE");
                }
                if(id.HasValue) {
                    var latest=ReadRow(conn,s,id.Value,true);
                    foreach(var field in original.Keys) if(!Same(latest[field],original[field])) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Dữ liệu đã được người khác sửa. Tải lại trước khi lưu.");
                    if(s.IsRequest && Convert.ToString(latest["TRANGTHAI"])!="PENDING") throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Đơn đã xử lý; không được sửa nội dung đã phê duyệt.");
                    recalculate = ProtectHistory(db,s,id.Value,values,original);
                }
                if(s.StartField!=null && Convert.ToString(values["TRANG_THAI"])=="ACTIVE") CheckOverlap(db,s,values,id);
                var data=new Dictionary<string,object>(values);
                if(s.IsSchedule) {
                    if(original!=null && (!Same(original["MANV"],data["MANV"]) || !Same(original["NGAY"],data["NGAY"]))) throw new ArgumentException("Không đổi nhân viên/ngày của lịch cũ; thêm lịch mới cho ngày khác.");
                    var employee=Convert.ToDecimal(data["MANV"]); var day=Convert.ToDateTime(data["NGAY"]).Date;
                    if(day.Year*100+day.Month!=period) throw new ArgumentException("Phân ca phải thuộc kỳ đang xử lý.");
                    var existing=PayrollPreparationSql.Query<ScheduleIdentity>(db,"SELECT IDLICH,MA_PHANCONG,SO_PHIENBAN FROM TB_LICH_LAMVIEC WHERE MANV=:m AND NGAY=:d AND TRANG_THAI='PUBLISHED'",new OracleParameter("m",employee),new OracleParameter("d",day)).ToList();
                    if(existing.Count>1) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Có nhiều lịch đang công bố cho ngày này. Cần đối chiếu trước khi sửa.");
                    if(original==null && existing.Count!=0) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Ngày đã có lịch. Chọn dòng hiện tại để sửa và tạo phiên bản thay thế.");
                    if(existing.Count==1) db.Database.ExecuteSqlCommand("UPDATE TB_LICH_LAMVIEC SET TRANG_THAI='SUPERSEDED' WHERE IDLICH=:p0",new OracleParameter("p0",existing[0].IDLICH));
                    data["MA_PHANCONG"]=existing.Count==1 ? existing[0].MA_PHANCONG : $"DESKTOP_{employee}_{day:yyyyMMdd}";
                    data["SO_PHIENBAN"]=existing.Count==1 ? existing[0].SO_PHIENBAN+1 : 1;
                    data["THAY_THE_ID"]=existing.Count==1 ? (object)existing[0].IDLICH : DBNull.Value;
                    data["TRANG_THAI"]="PUBLISHED"; data["NGUON_PHAN_CONG"]="DIEU_CHINH"; data["TAO_BOI"]=UserSession.CurrentUser.IDUSER;
                    if(Convert.ToString(data["TRANG_THAI_PHAN_CONG"])!="LAM_VIEC") {data["IDCAPHIENBAN"]=DBNull.Value;data["BATDAU_KEHOACH"]=DBNull.Value;data["KETTHUC_KEHOACH"]=DBNull.Value;}
                    id=null;
                }
                if(s.IsAttendanceConfiguration) {
                    ValidateAttendanceConfiguration(db,s,data,id);
                    if((s.Table=="TB_CA_PHIENBAN" || s.Table=="TB_QUYDINH_CHAMCONG") && !id.HasValue) data["TAO_BOI"]=UserSession.CurrentUser.IDUSER;
                }
                if(s.IsSchedule && Convert.ToString(data["TRANG_THAI_PHAN_CONG"])=="LAM_VIEC") {
                    var active=PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM TB_CA_PHIENBAN WHERE IDCAPHIENBAN=:id AND TRANG_THAI='PUBLISHED' AND TU_NGAY<=:d AND (DEN_NGAY IS NULL OR DEN_NGAY>:d)",new OracleParameter("id",data["IDCAPHIENBAN"]),new OracleParameter("d",data["NGAY"])).Single();
                    if(active!=1) throw new ArgumentException("Ca chưa công bố hoặc không hiệu lực ngày đã chọn. Hoàn tất Phiên bản ca và Khung giờ trước.");
                }
                if(s.IsRequest) PrepareRequest(s,data,period);
                if(s.Table=="TB_YEUCAU_TANGCA") {
                    var duplicate=PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM TB_YEUCAU_TANGCA WHERE MANV=:m AND TRUNC(NGAY)=:d AND NVL(IDCA,1)=:ca AND ID<>:id AND TRANGTHAI IN ('PENDING','APPROVED')",new OracleParameter("m",data["MANV"]),new OracleParameter("d",Convert.ToDateTime(data["NGAY"]).Date),new OracleParameter("ca",data["IDCA"]==DBNull.Value ? 1 : data["IDCA"]),new OracleParameter("id",id??-1)).Single();
                    if(duplicate>0) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Nhân viên đã có đơn tăng ca chờ duyệt/đã duyệt cho ngày và ca này. Tải lại, sửa đơn còn chờ hoặc xử lý đơn hiện có; không tạo đơn trùng.");
                }
                if(s.Table=="TB_DANG_KY_GIAM_TRU_NPT" && Convert.ToString(data["TRANG_THAI"])=="ACTIVE") {
                    decimal nptId=Convert.ToDecimal(data["ID_NPT"]);
                    int startM=Convert.ToInt32(data["THANG_BAT_DAU"]);
                    int endM=data["THANG_KET_THUC"]==DBNull.Value ? 999912 : Convert.ToInt32(data["THANG_KET_THUC"]);
                    var overlapCount=PayrollPreparationSql.Query<decimal>(db,@"
                        SELECT COUNT(*) FROM TB_DANG_KY_GIAM_TRU_NPT
                        WHERE ID_NPT=:npt AND ID<>:id AND TRANG_THAI='ACTIVE'
                          AND THANG_BAT_DAU<=:endM AND NVL(THANG_KET_THUC,999912)>=:startM",
                        new OracleParameter("npt",nptId),
                        new OracleParameter("id",id??-1),
                        new OracleParameter("endM",endM),
                        new OracleParameter("startM",startM)).Single();
                    if(overlapCount>0)
                        throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Người phụ thuộc đã được nhân viên khác đăng ký trong khoảng thời gian chồng lấn. Chỉ được chuyển người khai ở khoảng thời gian không chồng lấn.");
                }
                if(s.Table=="TB_NGUOI_PHU_THUOC" && Convert.ToString(data["TRANG_THAI"])=="ACTIVE") {
                    int startM=Convert.ToInt32(data["THANG_BAT_DAU_GIAM_TRU"]);
                    int endM=data["THANG_KET_THUC_GIAM_TRU"]==DBNull.Value ? 999912 : Convert.ToInt32(data["THANG_KET_THUC_GIAM_TRU"]);
                    decimal manv=Convert.ToDecimal(data["MANV"]);
                    string cccd=data["CCCD"]==DBNull.Value ? null : Convert.ToString(data["CCCD"]).Trim();
                    string mst=data["MA_SO_THUE_NPT"]==DBNull.Value ? null : Convert.ToString(data["MA_SO_THUE_NPT"]).Trim();
                    string ddcn=data["MA_DINH_DANH_CA_NHAN"]==DBNull.Value ? null : Convert.ToString(data["MA_DINH_DANH_CA_NHAN"]).Trim();
                    string soGt=data["SO_GIAY_TO"]==DBNull.Value ? null : Convert.ToString(data["SO_GIAY_TO"]).Trim();
                    string loaiGt=data["LOAI_GIAY_TO"]==DBNull.Value ? null : Convert.ToString(data["LOAI_GIAY_TO"]).Trim();

                    if(!string.IsNullOrEmpty(cccd) || !string.IsNullOrEmpty(mst) || !string.IsNullOrEmpty(ddcn) || !string.IsNullOrEmpty(soGt)) {
                        var duplicateCount=PayrollPreparationSql.Query<decimal>(db,@"
                            SELECT COUNT(*) FROM TB_NGUOI_PHU_THUOC
                            WHERE MANV<>:m AND ID<>:id AND TRANG_THAI='ACTIVE'
                              AND THANG_BAT_DAU_GIAM_TRU<=:endM AND NVL(THANG_KET_THUC_GIAM_TRU,999912)>=:startM
                              AND (
                                (:c1 IS NOT NULL AND CCCD=:c1)
                                OR (:m1 IS NOT NULL AND MA_SO_THUE_NPT=:m1)
                                OR (:d1 IS NOT NULL AND MA_DINH_DANH_CA_NHAN=:d1)
                                OR (:s1 IS NOT NULL AND LOAI_GIAY_TO=:l1 AND SO_GIAY_TO=:s1)
                              )",
                            new OracleParameter("m",manv),
                            new OracleParameter("id",id??-1),
                            new OracleParameter("endM",endM),
                            new OracleParameter("startM",startM),
                            new OracleParameter("c1",(object)cccd??DBNull.Value),
                            new OracleParameter("m1",(object)mst??DBNull.Value),
                            new OracleParameter("d1",(object)ddcn??DBNull.Value),
                            new OracleParameter("s1",(object)soGt??DBNull.Value),
                            new OracleParameter("l1",(object)loaiGt??DBNull.Value)).Single();
                        if(duplicateCount>0)
                            throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Người phụ thuộc này đã được nhân viên khác đăng ký với cùng số định danh/giấy tờ trong khoảng thời gian chồng lấn.");
                    }
                }
                if(!id.HasValue && s.Sequence!=null) data[key]=PayrollPreparationSql.Query<decimal>(db,$"SELECT {s.Sequence}.NEXTVAL FROM DUAL").Single();
                if(table.StartsWith("TB_NHANVIEN_") || table=="TB_NGUOI_PHU_THUOC") {
                    var metadata=PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM ALL_TAB_COLUMNS WHERE OWNER=SYS_CONTEXT('USERENV','CURRENT_SCHEMA') AND TABLE_NAME=:t AND COLUMN_NAME IN ('DATA_ORIGIN','SOURCE_NOTE')",new OracleParameter("t",table)).Single();
                    if(metadata==2) {data["DATA_ORIGIN"]=original==null ? "MANUAL" : ReadRow(conn,s,id.Value,false)["DATA_ORIGIN"]; data["SOURCE_NOTE"]="Desktop #"+UserSession.CurrentUser.IDUSER+": "+reason.Substring(0,Math.Min(reason.Length,200));}
                }
                Write(conn,s,data,id);
                if(recalculate.Length>0) {
                    // Only open, unapproved snapshots passed the history guard. Preserve all salary amounts;
                    // mark their inputs stale in the same transaction as the source-profile edit.
                    db.Database.ExecuteSqlCommand($"UPDATE TB_BANGLUONG SET TRANG_THAI='CAN_TINH_LAI' WHERE {s.ReferenceField}=:refId AND TRANG_THAI IN ('DRAFT','CALCULATED','CAN_TINH_LAI')",
                        new OracleParameter("refId",id.Value));
                    Audit(db,table,id.Value.ToString(),"PROFILE_CHANGE_REQUIRES_RECALC",string.Join(",",recalculate)+": "+reason);
                }
                if(s.IsAttendanceConfiguration) db.Database.ExecuteSqlCommand("UPDATE TB_KYCONG SET CONG_INPUT_REV=NVL(CONG_INPUT_REV,0)+1 WHERE NVL(KHOA,0)=0");
                if(s.IsSchedule || s.IsRequest) db.Database.ExecuteSqlCommand("UPDATE TB_KYCONG SET CONG_INPUT_REV=NVL(CONG_INPUT_REV,0)+1 WHERE MAKYCONG=:p0",new OracleParameter("p0",period));
                Audit(db,table,id.HasValue?id.Value.ToString():"NEW","PAYROLL_PREPARATION_SAVE",reason);
                if(validateOnly) tx.Rollback(); else tx.Commit();
                return recalculate;
            }
        }
        private static void PrepareRequest(PayrollDataSection s,Dictionary<string,object> data,int period)
        {
            var day=Convert.ToDateTime(data[s.Table=="TB_YEUCAU_NGHIPHEP"?"TUNGAY":"NGAY"]);
            if(day.Year*100+day.Month!=period) throw new ArgumentException("Yêu cầu phải bắt đầu trong kỳ đang xử lý; tách đơn nếu qua nhiều kỳ.");
            foreach(var name in s.Table=="TB_YEUCAU_DIEUCHINHCONG" ? new[]{"THOIDIEM_VAO_MOI","THOIDIEM_RA_MOI"} : s.Table=="TB_YEUCAU_TANGCA" ? new[]{"BATDAU_DANGKY","KETTHUC_DANGKY"} : new[]{"BATDAU_NGHI","KETTHUC_NGHI","GIAY_NGHI","NGUON_CHI_TRA","LOAI_HUONG_CONG"})
                if(!data.ContainsKey(name) || data[name]==DBNull.Value) throw new ArgumentException("Thiếu khoảng thời gian hoặc loại hưởng công của yêu cầu.");
            data["TRANGTHAI"]="PENDING";
            if(s.Table=="TB_YEUCAU_DIEUCHINHCONG") {var a=Convert.ToDateTime(data["THOIDIEM_VAO_MOI"]);var b=Convert.ToDateTime(data["THOIDIEM_RA_MOI"]);
                if(b<=a || a.Date!=day.Date || (b-a).TotalHours>24) throw new ArgumentException("Giờ điều chỉnh phải cùng ngày công và kết thúc sau bắt đầu, tối đa 24 giờ.");
                data["GIO_VAO"]=a.ToString("HH:mm");data["GIO_RA"]=b.ToString("HH:mm");}
            if(s.Table=="TB_YEUCAU_TANGCA") {var a=Convert.ToDateTime(data["BATDAU_DANGKY"]);var b=Convert.ToDateTime(data["KETTHUC_DANGKY"]);
                if(b<=a || a.Date!=day.Date || (b-a).TotalHours>24 || Math.Abs((decimal)(b-a).TotalHours-Convert.ToDecimal(data["GIOTANGCA"]))>0.01m) throw new ArgumentException("Khoảng tăng ca phải khớp ngày và số giờ đăng ký.");}
            if(s.Table=="TB_YEUCAU_NGHIPHEP") {var a=Convert.ToDateTime(data["BATDAU_NGHI"]);var b=Convert.ToDateTime(data["KETTHUC_NGHI"]);
                if(b<=a || a.Date!=day.Date || Convert.ToDateTime(data["DENNGAY"]).Date.Year*100+Convert.ToDateTime(data["DENNGAY"]).Month!=period || b.AddTicks(-1).Date.Year*100+b.AddTicks(-1).Month!=period || Convert.ToDecimal(data["GIAY_NGHI"])<0 || Convert.ToDecimal(data["GIAY_NGHI"])>(decimal)(b-a).TotalSeconds) throw new ArgumentException("Kiểm tra khoảng nghỉ trong kỳ và số giây nghỉ theo lịch làm việc.");}
        }
        public void DecideRequest(string table,decimal id,int period,bool approve,string reason)
        {
            var s=Find(table); if(!s.IsRequest) throw new ArgumentException("Chỉ phê duyệt đơn yêu cầu."); Demand(table,false,true);
            if(string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Phải ghi căn cứ phê duyệt/từ chối.");
            using(var db=new MyEntities()) using(var tx=db.Database.BeginTransaction()) {
                CheckOpenPeriod(db,period); var row=ReadRow((OracleConnection)db.Database.Connection,s,id,true);
                if(Convert.ToString(row["TRANGTHAI"])!="PENDING") throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Đơn đã được người khác xử lý.");
                decimal employee=Convert.ToDecimal(row["MANV"]); var actor=UserSession.CurrentUser.IDUSER;
                var mapping=PayrollPreparationSql.Query<decimal?>(db,"SELECT EMPLOYEE_ID FROM TB_USER_EMPLOYEE_MAPPING WHERE USER_ID=:p0",new OracleParameter("p0",actor)).FirstOrDefault();
                if(mapping==employee) throw new UnauthorizedAccessException("Không được tự phê duyệt/từ chối đơn của chính mình.");
                if(table=="TB_YEUCAU_DIEUCHINHCONG" && approve) {
                    var other=PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM TB_YEUCAU_DIEUCHINHCONG WHERE MANV=:m AND TRUNC(NGAY)=TRUNC(:d) AND TRANGTHAI='APPROVED' AND ID<>:id",new OracleParameter("m",employee),new OracleParameter("d",row["NGAY"]),new OracleParameter("id",id)).Single();
                    if(other>0) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Ngày này đã có điều chỉnh được duyệt. Cần đối chiếu/thu hồi quyết định trước, tránh chọn giờ tùy ý.");
                }
                var data=s.Fields.Split(',').ToDictionary(n=>n,n=>row[n]); PrepareRequest(s,data,period);
                data["TRANGTHAI"]=approve?"APPROVED":"REJECTED";data["NGUOIDUYET"]=actor;data["NGAYDUYET"]=DateTime.Now;data["GHICHUDUYET"]=reason;
                if(approve && table=="TB_YEUCAU_TANGCA") {data["BATDAU_DUYET"]=data["BATDAU_DANGKY"];data["KETTHUC_DUYET"]=data["KETTHUC_DANGKY"];data["GIAY_OT_DUYET"]=(decimal)(Convert.ToDateTime(data["KETTHUC_DANGKY"])-Convert.ToDateTime(data["BATDAU_DANGKY"])).TotalSeconds;}
                Write((OracleConnection)db.Database.Connection,s,data,id);
                db.Database.ExecuteSqlCommand("UPDATE TB_KYCONG SET CONG_INPUT_REV=NVL(CONG_INPUT_REV,0)+1 WHERE MAKYCONG=:p0",new OracleParameter("p0",period));
                Audit(db,table,id.ToString(),approve?"APPROVE_PAYROLL_INPUT":"REJECT_PAYROLL_INPUT",reason);tx.Commit();
            }
        }
        private static Dictionary<string,object> ReadRow(OracleConnection conn,PayrollDataSection s,decimal id,bool locking)
        {
            using(var cmd=new OracleCommand($"SELECT * FROM {s.Table} WHERE {Key(s)}=:id"+(locking?" FOR UPDATE":""),conn)) {cmd.BindByName=true;cmd.Parameters.Add("id",id);
                using(var r=cmd.ExecuteReader()) {if(!r.Read()) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Dòng đã bị xóa.");var d=new Dictionary<string,object>();for(int i=0;i<r.FieldCount;i++) d[r.GetName(i)]=r.GetValue(i);return d;}}
        }
        private static bool Same(object a,object b)=>a==DBNull.Value || b==DBNull.Value ? a==DBNull.Value && b==DBNull.Value : a is DateTime && b is DateTime ? (DateTime)a==(DateTime)b : Convert.ToString(a,System.Globalization.CultureInfo.InvariantCulture)==Convert.ToString(b,System.Globalization.CultureInfo.InvariantCulture);
        private static void Write(OracleConnection conn,PayrollDataSection s,Dictionary<string,object> values,decimal? id)
        {
            var fields=values.Keys.ToList();
            var sql=id.HasValue?$"UPDATE {s.Table} SET "+string.Join(",",fields.Select((f,i)=>f+"=:v"+i))+$" WHERE {Key(s)}=:id":$"INSERT INTO {s.Table} ("+string.Join(",",fields)+") VALUES ("+string.Join(",",fields.Select((f,i)=>":v"+i))+")";
            using(var cmd=new OracleCommand(sql,conn)) {cmd.BindByName=true;for(int i=0;i<fields.Count;i++) {var v=values[fields[i]]??DBNull.Value;var p=cmd.Parameters.Add("v"+i,v);if(v is string) p.OracleDbType=OracleDbType.NVarchar2;}if(id.HasValue)cmd.Parameters.Add("id",id.Value);cmd.ExecuteNonQuery();}
        }
        private static void CheckOpenPeriod(MyEntities db,int period)
        {
            var rows=PayrollPreparationSql.Query<decimal>(db,"SELECT NVL(KHOA,0) FROM TB_KYCONG WHERE MAKYCONG=:p0 FOR UPDATE",new OracleParameter("p0",period)).ToList();
            if(rows.Count!=1 || rows[0]!=0) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Kỳ công không tồn tại hoặc đã khóa.");
        }
        private static void CheckOverlap(MyEntities db,PayrollDataSection s,IDictionary<string,object> data,decimal? id)
        {
            var args=new List<OracleParameter>{new OracleParameter("a",data[s.StartField]),new OracleParameter("b",data[s.EndField]==DBNull.Value?(object)new DateTime(9999,12,31):data[s.EndField]),new OracleParameter("id",id??-1)};
            string extra="";
            if(data.ContainsKey("MANV")) {extra=" AND MANV=:m";args.Add(new OracleParameter("m",data["MANV"]));}
            if(data.ContainsKey("TAX_YEAR")) {extra=" AND TAX_YEAR=:y";args.Add(new OracleParameter("y",data["TAX_YEAR"]));}
            var count=PayrollPreparationSql.Query<decimal>(db,$"SELECT COUNT(*) FROM {s.Table} WHERE ID<>:id AND TRANG_THAI='ACTIVE' AND {s.StartField}<=:b AND NVL({s.EndField},DATE '9999-12-31')>=:a{extra}",args.ToArray()).Single();
            if(count>0) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Khoảng hiệu lực chồng lấn với bản đang hoạt động. Kết thúc phiên bản trước rồi thêm bản mới.");
        }
        public static bool IsReusableEmployeeProfile(PayrollDataSection section) => new[] {
            "TB_NHANVIEN_BAOHIEM_THAM_GIA", "TB_NHANVIEN_CONG_DOAN_THAM_GIA", "TB_NHANVIEN_THUE"
        }.Contains(section.Table);
        public static bool CanEditOpenProfile(PayrollDataSection section,IEnumerable<PayrollProfileUsage> usages)
        {
            return IsReusableEmployeeProfile(section) && usages.All(u => u.LOCKED==0 && u.LEGACY==0 && u.PAYMENT_BLOCKED==0
                && new[]{"DRAFT","CALCULATED","CAN_TINH_LAI"}.Contains(u.STATUS));
        }
        private static List<PayrollProfileUsage> ReadProfileUsage(MyEntities db,string reference,decimal id)
        {
            // Payment columns are optional on older deployments. Never reference a missing column.
            var paymentColumns=PayrollPreparationSql.Query<string>(db,@"SELECT TABLE_NAME||'.'||COLUMN_NAME FROM ALL_TAB_COLUMNS
                WHERE OWNER=SYS_CONTEXT('USERENV','CURRENT_SCHEMA') AND TABLE_NAME IN ('TB_KYCONG','TB_BANGLUONG') AND COLUMN_NAME='TRANGTHAI_CHITRA'").ToList();
            var payments=new List<string>();
            if(paymentColumns.Contains("TB_KYCONG.TRANGTHAI_CHITRA")) payments.Add("NVL(k.TRANGTHAI_CHITRA,'UNKNOWN')<>'CHUA_CHI_TRA'");
            if(paymentColumns.Contains("TB_BANGLUONG.TRANGTHAI_CHITRA")) payments.Add("NVL(b.TRANGTHAI_CHITRA,'UNKNOWN')<>'CHUA_CHI_TRA'");
            string payment=payments.Count==0 ? "0" : "CASE WHEN "+string.Join(" OR ",payments)+" THEN 1 ELSE 0 END";
            return PayrollPreparationSql.Query<PayrollProfileUsage>(db,$@"SELECT b.MAKYCONG, NVL(k.KHOA,1) LOCKED,
                NVL(b.IS_LEGACY,1) LEGACY, b.TRANG_THAI STATUS, {payment} PAYMENT_BLOCKED
                FROM TB_BANGLUONG b LEFT JOIN TB_KYCONG k ON k.MAKYCONG=b.MAKYCONG WHERE b.{reference}=:refId",
                new OracleParameter("refId",id)).ToList();
        }
        private static int[] ProtectHistory(MyEntities db,PayrollDataSection s,decimal id,IDictionary<string,object> values,IDictionary<string,object> original)
        {
            string reference=s.ReferenceField; decimal refId=id;
            if(s.Table=="TB_THUE_TNCN_BAC") {reference="POLICY_THUE_ID";refId=Convert.ToDecimal(original["POLICY_THUE_ID"]);}
            if(s.Table=="TB_CHINH_SACH_BHXH_VUNG") {reference="POLICY_BHXH_ID";refId=Convert.ToDecimal(original["POLICY_BHXH_ID"]);}
            if(reference==null) return new int[0];
            var usages=ReadProfileUsage(db,reference,refId);
            if(usages.Count==0) return new int[0];
            if(CanEditOpenProfile(s,usages)) {
                bool changed=values.Any(p=>!Same(p.Value,original[p.Key]));
                return changed ? usages.Select(u=>(int)u.MAKYCONG).Distinct().OrderBy(p=>p).ToArray() : new int[0];
            }
            if(s.EndField==null || values.Any(p=>p.Key!=s.EndField && !Same(p.Value,original[p.Key])))
                throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Hồ sơ/chính sách đã có kết quả lương cần bảo vệ. Chỉ kết thúc hiệu lực sau kỳ đã dùng, rồi thêm phiên bản mới; không phải nhập lại hồ sơ mỗi tháng.");
            var latest=(int)usages.Max(u=>u.MAKYCONG); var end=new DateTime(latest/100,latest%100,DateTime.DaysInMonth(latest/100,latest%100));
            if(values[s.EndField]!=DBNull.Value && Convert.ToDateTime(values[s.EndField])<end)
                throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Ngày kết thúc không được làm mất hiệu lực của kỳ đã tính lương.",s.EndField);
            return new int[0];
        }
        private static void Audit(MyEntities db,string table,string id,string action,string reason)
        {
            db.Database.ExecuteSqlCommand(@"INSERT INTO TB_SYS_LOG (MANV_THUCHIEN,TEN_THUCHIEN,HANHDONG,TEN_BANG,ID_BAN_GHI,DU_LIEU_MOI,IP_ADDRESS,TEN_MAY_TINH,THOIGIAN,MODULE_NAME,CHANGED_FIELDS)
                VALUES (:actor,:name,:action,:tab,:id,:detail,'127.0.0.1','DESKTOP_WINFORMS',SYSDATE,'PAYROLL_PREPARATION',:fields)",
                new OracleParameter("actor",UserSession.CurrentUser.IDUSER.ToString()),new OracleParameter("name",UserSession.CurrentUser.USERNAME),new OracleParameter("action",action),new OracleParameter("tab",table),new OracleParameter("id",id),new OracleParameter("detail",reason),new OracleParameter("fields",action));
        }
        private static void ValidateAttendanceConfiguration(MyEntities db,PayrollDataSection s,IDictionary<string,object> data,decimal? id)
        {
            if(data.ContainsKey("TRANG_THAI") && !new[]{"DRAFT","PUBLISHED","RETIRED"}.Contains(Convert.ToString(data["TRANG_THAI"]))) throw new ArgumentException("Chọn Nháp, Công bố hoặc Ngừng áp dụng.");
            if(s.Table=="TB_CA_KHUNGGIO") {
                var parent=PayrollPreparationSql.Query<string>(db,"SELECT TRANG_THAI FROM TB_CA_PHIENBAN WHERE IDCAPHIENBAN=:id",new OracleParameter("id",data["IDCAPHIENBAN"])).Single();
                if(parent!="DRAFT") throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Chỉ nhập khung giờ cho phiên bản ca nháp. Tạo phiên bản ca mới để thay đổi ca đã công bố.");
            }
            if(id.HasValue && s.Table!="TB_CA_KHUNGGIO") {
                var state=PayrollPreparationSql.Query<string>(db,$"SELECT TRANG_THAI FROM {s.Table} WHERE {Key(s)}=:id",new OracleParameter("id",id.Value)).Single();
                if(state!="DRAFT") {
                    var original=ReadRow((OracleConnection)db.Database.Connection,s,id.Value,false);
                    bool onlyEnd=s.Table=="TB_HESO_TANGCA" && data["DEN_NGAY"]!=DBNull.Value && data.All(p=>p.Key=="DEN_NGAY" || Same(p.Value,original[p.Key]));
                    if(!onlyEnd) throw new BusinessException("PAYROLL_PREPARATION_BLOCKED", "Phiên bản đã công bố được giữ nguyên. Thêm phiên bản mới với thời hạn riêng để thay đổi.");
                    var used=PayrollPreparationSql.Query<decimal>(db,"SELECT MAKYCONG FROM TB_BANGLUONG WHERE IS_LEGACY=0");
                    if(used.Count>0) {int latest=(int)used.Max();if(Convert.ToDateTime(data["DEN_NGAY"])<=new DateTime(latest/100,latest%100,1).AddMonths(1).AddDays(-1)) throw new ArgumentException("Hệ số tăng ca chỉ được kết thúc sau kỳ lương đã tính. Không sửa mức/hệ số lịch sử.");}
                }
            }
            if(s.Table=="TB_CA_PHIENBAN" && Convert.ToString(data["TRANG_THAI"])=="PUBLISHED") {
                if(!id.HasValue) throw new ArgumentException("Lưu ca ở trạng thái Nháp, nhập đủ khung giờ rồi công bố.");
                var total=PayrollPreparationSql.Query<decimal>(db,"SELECT NVL(SUM(CASE WHEN LOAI_KHUNGGIO IN ('LAM_VIEC','NGHI_HUONG_LUONG') THEN (KETTHUC_PHUT-BATDAU_PHUT)*60 ELSE 0 END),0) FROM TB_CA_KHUNGGIO WHERE IDCAPHIENBAN=:id",new OracleParameter("id",id.Value)).Single();
                var overlaps=PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM TB_CA_KHUNGGIO a JOIN TB_CA_KHUNGGIO b ON a.IDCAPHIENBAN=b.IDCAPHIENBAN AND a.IDKHUNGGIO<b.IDKHUNGGIO AND a.BATDAU_PHUT<b.KETTHUC_PHUT AND b.BATDAU_PHUT<a.KETTHUC_PHUT WHERE a.IDCAPHIENBAN=:id",new OracleParameter("id",id.Value)).Single();
                if(total!=Convert.ToDecimal(data["TONG_GIAY_CHUAN"]) || overlaps>0) throw new ArgumentException("Khung giờ phải không chồng lấn và tổng thời gian được hưởng công phải bằng giờ chuẩn ca.");
            }
            if(s.Table=="TB_QUYDINH_CHAMCONG" && Convert.ToString(data["KIEU_LAM_TRON"])!="EXACT") throw new ArgumentException("Bộ tính công hiện hỗ trợ thời gian thực, chưa hỗ trợ làm tròn. Chọn EXACT và bước 1 giây.");
        }
        public static void ValidateSchedule(IDictionary<string,object> values,int period)
        {
            var day=Convert.ToDateTime(values["NGAY"]).Date;
            if(day.Year*100+day.Month!=period) throw new ArgumentException("Lịch phải thuộc kỳ đã chọn.");
            if(!new[]{"LAM_VIEC","NGHI"}.Contains(Convert.ToString(values["TRANG_THAI_PHAN_CONG"]))) throw new ArgumentException("Chọn làm việc hoặc ngày nghỉ.");
            if(Convert.ToString(values["TRANG_THAI_PHAN_CONG"])=="LAM_VIEC") {
                foreach(var field in new[]{"IDCAPHIENBAN","IDQUYDINH","BATDAU_KEHOACH","KETTHUC_KEHOACH"}) if(values[field]==DBNull.Value) throw new ArgumentException("Ngày làm việc phải có ca, quy định và giờ kế hoạch.");
                var start=Convert.ToDateTime(values["BATDAU_KEHOACH"]); var end=Convert.ToDateTime(values["KETTHUC_KEHOACH"]);
                if(start.Date!=day || end<=start || (end-start).TotalHours>24) throw new ArgumentException("Giờ kế hoạch không hợp lệ; ca qua đêm nhập ngày kết thúc kế tiếp.");
            }
        }
        public sealed class ScheduleIdentity { public decimal IDLICH { get; set; } public string MA_PHANCONG { get; set; } public decimal SO_PHIENBAN { get; set; } }
    }
    internal static class PayrollPreparationSql
    {
        internal static List<T> Query<T>(MyEntities db,string sql,params OracleParameter[] supplied)
        {
            var args=new List<object>(); var byName=supplied.ToDictionary(p=>p.ParameterName.TrimStart(':'),StringComparer.OrdinalIgnoreCase);
            sql=System.Text.RegularExpressions.Regex.Replace(sql,@":([A-Za-z][A-Za-z0-9_]*)",m=>{
                var original=byName[m.Groups[1].Value]; var name="bound"+args.Count;
                var copy=new OracleParameter(name,original.OracleDbType) {Value=original.Value??DBNull.Value};args.Add(copy);return ":"+name;
            });
            return db.Database.SqlQuery<T>(sql,args.ToArray()).ToList();
        }
    }

}
