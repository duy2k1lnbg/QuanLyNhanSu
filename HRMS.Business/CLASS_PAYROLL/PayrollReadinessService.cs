using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Bu.CLASS_PAYROLL
{
    public enum PayrollPreparationTarget { Period, Contracts, Policies, Insurance, Union, Tax, Attendance, PublishAttendance, Occurrences, Database }
    public sealed class PayrollPreparationStep
    {
        public string Title { get; set; }
        public string Status { get; set; }
        public string Guidance { get; set; }
        public bool Blocking { get; set; }
        public PayrollPreparationTarget Target { get; set; }
        public List<PayrollPreparationIssue> Issues { get; set; } = new List<PayrollPreparationIssue>();
    }
    public sealed class PayrollPreparationIssue
    {
        public decimal EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? Day { get; set; }
        public string Reason { get; set; }
        public string ActionTable { get; set; }
    }
    public sealed class PayrollReadinessReport
    {
        public int Period { get; set; }
        public DateTime CheckedAt { get; set; }
        public List<PayrollPreparationStep> Steps { get; set; } = new List<PayrollPreparationStep>();
        public bool CanCalculate => Steps.Count > 0 && !Steps.Any(s => s.Blocking);
    }
    // Read-only preflight; the engine still rechecks revisions and locks at commit.
    public sealed class PayrollReadinessService
    {
        public sealed class EmployeeRow { public decimal MANV { get; set; } public string HOTEN { get; set; } public decimal? BASE_SALARY { get; set; } public DateTime CONTRACT_START { get; set; } public DateTime? CONTRACT_END { get; set; } }
        public sealed class PolicyRow { public decimal ID { get; set; } }
        public sealed class ProfileRow { public decimal MANV { get; set; } public DateTime START_DATE { get; set; } public DateTime? END_DATE { get; set; } }
        public sealed class DayRow { public decimal MANV { get; set; } public string HOTEN { get; set; } public DateTime NGAY { get; set; } public decimal UNPUBLISHED { get; set; } public decimal UNRESOLVED { get; set; } public decimal STALE { get; set; } }
        public sealed class AnomalyRow {public decimal MANV {get;set;}public DateTime NGAY {get;set;}public string MA_LOI {get;set;}public string MO_TA {get;set;}}
        public sealed class TaxProfileRow { public decimal ID { get; set; } public decimal MANV { get; set; } public string MA_SO_THUE { get; set; } public string LOAI_DINH_DANH { get; set; } public string SO_HO_CHIEU { get; set; } public decimal IS_CU_TRU { get; set; } public string CAN_CU_CU_TRU { get; set; } public decimal CO_UY_QUYEN_QUYET_TOAN { get; set; } public string TRANG_THAI_HO_SO { get; set; } public DateTime NGAY_HIEU_LUC { get; set; } public DateTime? NGAY_HET_HIEU_LUC { get; set; } }
        public sealed class DependentRow { public decimal ID { get; set; } public decimal MANV { get; set; } public string HO_TEN { get; set; } public string MOI_QUAN_HE { get; set; } public string CCCD { get; set; } public string MA_SO_THUE_NPT { get; set; } public DateTime? NGAY_SINH { get; set; } public string LOAI_GIAY_TO { get; set; } public string SO_GIAY_TO { get; set; } public string MA_DINH_DANH_CA_NHAN { get; set; } public string CAN_CU_GIAM_TRU { get; set; } public string TRANG_THAI_DANG_KY { get; set; } public string TINH_TRANG_HO_SO_CHUNG_MINH { get; set; } public decimal THANG_BAT_DAU_GIAM_TRU { get; set; } public decimal? THANG_KET_THUC_GIAM_TRU { get; set; } }
        public sealed class DependentRegistrationRow { public decimal ID { get; set; } public decimal ID_NPT { get; set; } public decimal MANV { get; set; } public decimal THANG_BAT_DAU { get; set; } public decimal? THANG_KET_THUC { get; set; } public string TRANG_THAI { get; set; } }
        public static string NormalizeName(string name) {
            if (string.IsNullOrWhiteSpace(name)) return "";
            var normalized = name.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized) {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(c);
            }
            return System.Text.RegularExpressions.Regex.Replace(sb.ToString().Normalize(System.Text.NormalizationForm.FormC).ToUpperInvariant().Trim(), @"\s+", " ");
        }
        public static string AttendanceAction(string code)=>code=="VANG_CHUA_XAC_NHAN" ? "TB_YEUCAU_NGHIPHEP" : code=="THIEU_GIO_VAO" || code=="THIEU_GIO_RA" ? "TB_YEUCAU_DIEUCHINHCONG" : code=="OT_CHUA_DUYET" || code=="OT_VUOT_GIAY_DUYET" ? "TB_YEUCAU_TANGCA" : code=="CHAM_NGOAI_LICH" || code=="THIEU_CAU_HINH" ? "TB_LICH_LAMVIEC" : "TB_CHAMCONG_BATTHUONG";
        public static string AttendanceAdvice(string code)=>code=="VANG_CHUA_XAC_NHAN" ? "Đối chiếu vắng mặt: lập đơn nghỉ đúng loại hưởng công; nếu quên quẹt thẻ thì lập Điều chỉnh giờ công có căn cứ." : code=="THIEU_GIO_VAO" || code=="THIEU_GIO_RA" ? "Bổ sung giờ vào/ra thực tế bằng đơn Điều chỉnh giờ công rồi phê duyệt." : code=="OT_CHUA_DUYET" || code=="OT_VUOT_GIAY_DUYET" ? "Đối chiếu khoảng tăng ca, lập/sửa đơn và phê duyệt đúng giờ. Nếu không phải tăng ca, xác minh Không tính tại Xác minh bất thường." : code=="CHAM_NGOAI_LICH" ? "Đối chiếu phân ca/ngày nghỉ. Sửa lịch nếu đã làm việc thực tế; nếu không tính công, xác minh tại Xác minh bất thường." : code=="THIEU_CAU_HINH" ? "Bổ sung lịch làm việc, ca đã công bố và quy định tính công; kiểm tra Phiên bản ca / Khung giờ." : "Đối chiếu nguồn công và nội dung bất thường trước khi công bố lại.";
        public static bool IsValidPeriod(int p) => p/100>=2000 && p/100<=2100 && p%100>=1 && p%100<=12;
        public static DateTime ProfileDate(EmployeeRow e, DateTime end) => e.CONTRACT_END.HasValue && e.CONTRACT_END.Value<end ? e.CONTRACT_END.Value : end;
        public static bool ContractsOverlap(DateTime a, DateTime? b, DateTime c, DateTime? d) => a<=(d??DateTime.MaxValue) && c<=(b??DateTime.MaxValue);
        public static int EffectiveProfileCount(IEnumerable<ProfileRow> rows, decimal manv, DateTime date) => rows.Count(p=>p.MANV==manv && p.START_DATE<=date && (!p.END_DATE.HasValue || p.END_DATE>=date));
        public PayrollReadinessReport Inspect(int period)
        {
            var report=new PayrollReadinessReport { Period=period, CheckedAt=DateTime.Now };
            if(!IsValidPeriod(period)) { Add(report,"Kỳ công",PayrollPreparationTarget.Period,true,"Chọn tháng và năm hợp lệ."); return report; }
            var start=new DateTime(period/100,period%100,1); var end=start.AddMonths(1).AddDays(-1);
            try {
                using(var db=new MyEntities()) {
                    var kc=db.TB_KYCONG.AsNoTracking().FirstOrDefault(k=>k.MAKYCONG==period);
                    bool isFrozen = db.Database.SqlQuery<decimal>("SELECT NVL(LUONG_CONGBO_LOCKED,0) FROM TB_KYCONG WHERE MAKYCONG=:p0", new OracleParameter("p0", period)).FirstOrDefault() == 1;
                    bool periodBlocked = kc == null || isFrozen || (kc.KHOA ?? 0) != 1;
                    Add(report, "1. Kỳ công", PayrollPreparationTarget.Period, periodBlocked,
                        kc == null ? "Mở Bảng công, thêm kỳ " + period + "." :
                        isFrozen ? "Lương đã công bố và khóa bất biến. Không được tính toán lại." :
                        (kc.KHOA ?? 0) != 1 ? "Bảng công chưa khóa sổ. Cần mở Bảng công và khóa kỳ công trước khi tính lương." :
                        "Bảng công đã khóa sổ. Sẵn sàng tính lương.");
                    InspectPolicies(db,report,end);
                    if(kc==null) return report;
                    var employees=PayrollPreparationSql.Query<EmployeeRow>(db,@"
                      SELECT MANV,HOTEN,BASE_SALARY,CONTRACT_START,CONTRACT_END FROM (
                        SELECT nv.MANV,nv.HOTEN,NVL(lh.LUONG_THANG,hd.LUONG_THOA_THUAN) BASE_SALARY,
                          hd.NGAYBATDAU CONTRACT_START,hd.NGAYKETTHUC CONTRACT_END,
                          ROW_NUMBER() OVER(PARTITION BY nv.MANV ORDER BY hd.NGAYBATDAU DESC,hd.SOHD DESC) RN
                        FROM TB_NHANVIEN nv JOIN TB_HOPDONG hd ON hd.MANV=nv.MANV
                        LEFT JOIN (SELECT MANV,LUONG_THANG,ROW_NUMBER() OVER(PARTITION BY MANV ORDER BY TU_NGAY DESC,ID DESC) RN_LH
                          FROM TB_LUONG_HIEULUC WHERE TU_NGAY<=:p_end AND (DEN_NGAY IS NULL OR DEN_NGAY>=:p_start)) lh ON lh.MANV=nv.MANV AND lh.RN_LH=1
                        WHERE (nv.DELETED_DATE IS NULL OR nv.DELETED_DATE>=:p_start) AND hd.DEL_DATE IS NULL
                          AND hd.NGAYBATDAU<=:p_end AND (hd.NGAYKETTHUC IS NULL OR hd.NGAYKETTHUC>=:p_start)
                      ) WHERE RN=1 ORDER BY MANV",new OracleParameter("p_end",end),new OracleParameter("p_start",start)).ToList();
                    var issues=employees.Where(e=>!e.BASE_SALARY.HasValue || e.BASE_SALARY<=0).Select(e=>Issue(e,"Thiếu mức lương hợp đồng/lương hiệu lực lớn hơn 0.")).ToList();
                    var overlaps=PayrollPreparationSql.Query<decimal>(db,@"
                      SELECT DISTINCT a.MANV FROM TB_HOPDONG a JOIN TB_HOPDONG b ON a.MANV=b.MANV AND a.SOHD<b.SOHD
                      WHERE a.DEL_DATE IS NULL AND b.DEL_DATE IS NULL AND a.NGAYBATDAU<=:p_end AND b.NGAYBATDAU<=:p_end
                        AND NVL(a.NGAYKETTHUC,DATE '9999-12-31')>=:p_start AND NVL(b.NGAYKETTHUC,DATE '9999-12-31')>=:p_start
                        AND a.NGAYBATDAU<=NVL(b.NGAYKETTHUC,DATE '9999-12-31') AND b.NGAYBATDAU<=NVL(a.NGAYKETTHUC,DATE '9999-12-31')",
                        new OracleParameter("p_end",end),new OracleParameter("p_start",start)).ToList();
                    issues.AddRange(employees.Where(e=>overlaps.Contains(e.MANV)).Select(e=>Issue(e,"Hợp đồng chồng lấn thời gian hiệu lực.")));
                    Add(report,"3. Hợp đồng và mức lương",PayrollPreparationTarget.Contracts,employees.Count==0 || issues.Count>0,
                        employees.Count==0 ? "Chưa có nhân viên có hợp đồng trong kỳ. Mở Hợp đồng lao động để khai báo." : $"{employees.Count} nhân viên có hợp đồng trong kỳ. Nhân viên không có hợp đồng không được tự thêm vào bảng lương.",issues);
                    InspectProfiles(db,report,employees,end,"TB_NHANVIEN_BAOHIEM_THAM_GIA","NGAY_BAT_DAU","NGAY_KET_THUC",PayrollPreparationTarget.Insurance,"4. Hồ sơ bảo hiểm");
                    InspectProfiles(db,report,employees,end,"TB_NHANVIEN_CONG_DOAN_THAM_GIA","NGAY_GIA_NHAP","NGAY_KET_THUC",PayrollPreparationTarget.Union,"5. Hồ sơ công đoàn");
                    InspectTaxAndDependents(db,report,employees,start,end,period);
                    var days=PayrollPreparationSql.Query<DayRow>(db,@"
                      SELECT b.MANV,NVL(n.HOTEN,b.HOTEN) HOTEN,b.NGAY,
                        CASE WHEN b.LANTINH_ID_HIENHANH IS NULL THEN 1 ELSE 0 END UNPUBLISHED,
                        CASE WHEN b.LANTINH_ID_HIENHANH IS NOT NULL AND NVL(b.DU_DIEUKIEN_CHOT,0)=0 THEN 1 ELSE 0 END UNRESOLVED,
                        CASE WHEN b.LANTINH_ID_HIENHANH IS NOT NULL AND (r.IDLANTINH IS NULL OR r.INPUT_REV<>NVL(k.CONG_INPUT_REV,0)) THEN 1 ELSE 0 END STALE
                      FROM TB_BANGCONG_CHITIET b JOIN TB_KYCONG k ON k.MAKYCONG=b.MAKYCONG
                      LEFT JOIN TB_CHAMCONG_LANTINH r ON r.IDLANTINH=b.LANTINH_ID_HIENHANH
                      LEFT JOIN TB_NHANVIEN n ON n.MANV=b.MANV WHERE b.MAKYCONG=:p0 ORDER BY b.NGAY,b.MANV",new OracleParameter("p0",period)).ToList();
                    var anomalies=PayrollPreparationSql.Query<AnomalyRow>(db,"SELECT a.MANV,a.NGAY,a.MA_LOI,a.MO_TA FROM TB_CHAMCONG_BATTHUONG a JOIN TB_BANGCONG_CHITIET b ON b.IDBANGCONGCT=a.IDBANGCONGCT WHERE b.MAKYCONG=:period AND NVL(b.DU_DIEUKIEN_CHOT,0)=0 AND a.TRANG_THAI='CHO_XU_LY'",new OracleParameter("period",period));
                    var attendanceIssues=days.Where(d=>d.UNRESOLVED==1).Select(d=> {
                        var cases=anomalies.Where(a=>a.MANV==d.MANV && a.NGAY.Date==d.NGAY.Date).ToList();
                        return new PayrollPreparationIssue {EmployeeId=d.MANV,EmployeeName=d.HOTEN,Day=d.NGAY,
                            Reason=cases.Count==0 ? "Ngày chưa đủ điều kiện chốt. Kiểm tra nguồn và công bố lại." : string.Join("\n",cases.Select(a=>a.MO_TA+" "+AttendanceAdvice(a.MA_LOI)).Distinct()),
                            ActionTable=cases.Count==0 ? "TB_LICH_LAMVIEC" : AttendanceAction(cases[0].MA_LOI)};
                    }).ToList();
                    attendanceIssues.AddRange(employees.Where(e=>!days.Any(d=>d.MANV==e.MANV)).Select(e=>Issue(e,"Chưa có ngày công trong kỳ.")));
                    Add(report,"7. Xử lý công và đơn yêu cầu",PayrollPreparationTarget.Attendance,days.Count==0,
                        $"{attendanceIssues.Count} mục cần xử lý. Chọn nhân viên/ngày rồi cập nhật công, phân ca hoặc lập và phê duyệt đơn có căn cứ.",attendanceIssues);
                    var publishIssues=days.Where(d=>d.UNPUBLISHED==1 || d.STALE==1).Select(d=>new PayrollPreparationIssue { EmployeeId=d.MANV,EmployeeName=d.HOTEN,Day=d.NGAY,Reason=d.UNPUBLISHED==1 ? "Chưa công bố ngày công." : "Nguồn công thay đổi sau lần công bố." }).ToList();
                    Add(report,"8. Công bố bảng công",PayrollPreparationTarget.PublishAttendance,days.Count==0,
                        $"{publishIssues.Count} ngày cần công bố lại. Công bố đọc nguồn đã xác nhận; không tự duyệt đơn hoặc xóa bất thường.",publishIssues);
                    var otIssues=PayrollPreparationSql.Query<PayrollPreparationIssue>(db,@"
                      SELECT b.MANV EmployeeId,b.HOTEN EmployeeName,b.NGAY Day,'Thiếu hoặc trùng hệ số tăng ca hiệu lực cho ngày/ca này.' Reason
                      FROM TB_CONG_PHANDOAN p JOIN TB_BANGCONG_CHITIET b ON b.IDBANGCONGCT=p.IDBANGCONGCT AND b.LANTINH_ID_HIENHANH=p.IDLANTINH
                      WHERE b.MAKYCONG=:period AND p.GIAY_OT_XAC_NHAN>0 AND (SELECT COUNT(*) FROM TB_HESO_TANGCA h
                        WHERE h.TRANG_THAI='PUBLISHED' AND h.LOAICONG=p.LOAI_NGAY AND h.LA_BAN_DEM=p.LA_BAN_DEM
                        AND (h.CO_OT_BAN_NGAY_TRUOC IS NULL OR h.CO_OT_BAN_NGAY_TRUOC=p.CO_OT_BAN_NGAY_TRUOC)
                        AND h.TU_NGAY<=p.BATDAU_LUC AND (h.DEN_NGAY IS NULL OR p.BATDAU_LUC<h.DEN_NGAY))<>1",new OracleParameter("period",period));
                    var otCount=PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM TB_CONG_PHANDOAN p JOIN TB_BANGCONG_CHITIET b ON b.IDBANGCONGCT=p.IDBANGCONGCT AND b.LANTINH_ID_HIENHANH=p.IDLANTINH WHERE b.MAKYCONG=:period AND p.GIAY_OT_XAC_NHAN>0",new OracleParameter("period",period)).Single();
                    if(otCount>0 && PayrollPreparationSql.Query<decimal>(db,"SELECT COUNT(*) FROM TB_CHINH_SACH_LUONG WHERE TRANG_THAI='ACTIVE' AND NGAY_HIEU_LUC<=:d AND (NGAY_HET_HIEU_LUC IS NULL OR NGAY_HET_HIEU_LUC>=:d)",new OracleParameter("d",start)).Single()!=1)
                        otIssues.Add(new PayrollPreparationIssue {Reason="Tính đơn giá tăng ca cần đúng một chính sách lương hiệu lực đầu kỳ."});
                    Add(report,"9. Đơn giá và hệ số tăng ca",PayrollPreparationTarget.Policies,otIssues.Count>0,"Mở nơi xử lý, chọn Hệ số trả lương tăng ca. Mỗi phân đoạn công bố phải có đúng một quy định trả lương hiệu lực.",otIssues);
                    Add(report,"10. Phụ cấp, phát sinh và tạm ứng",PayrollPreparationTarget.Occurrences,false,
                        "Rà soát thu nhập/khấu trừ đúng kỳ. Khoản nháp chưa duyệt không được tính; phụ cấp hợp đồng quản lý tại Phụ cấp, tạm ứng tại Ứng lương.");
                }
            } catch(Exception error) {
                var reference="PAYCHECK-"+Guid.NewGuid().ToString("N").Substring(0,10);
                System.Diagnostics.Trace.TraceError("[{0}] Payroll readiness: {1}",reference,error);
                Add(report,"Không đọc đủ dữ liệu kiểm tra",PayrollPreparationTarget.Database,true,"Kiểm tra kết nối/schema và quyền truy cập Oracle. Không thể kết luận đủ điều kiện khi truy vấn lỗi. Mã đối chiếu: "+reference);
            }
            return report;
        }
        private static void InspectTaxAndDependents(MyEntities db, PayrollReadinessReport report, List<EmployeeRow> employees, DateTime start, DateTime end, int period)
        {
            var taxProfiles = PayrollPreparationSql.Query<TaxProfileRow>(db, @"
                SELECT ID, MANV, MA_SO_THUE, NVL(LOAI_DINH_DANH, 'CCCD') LOAI_DINH_DANH, SO_HO_CHIEU,
                       NVL(IS_CU_TRU, 1) IS_CU_TRU, NVL(CAN_CU_CU_TRU, 'CHUA_XAC_MINH') CAN_CU_CU_TRU,
                       NVL(CO_UY_QUYEN_QUYET_TOAN, 0) CO_UY_QUYEN_QUYET_TOAN,
                       NVL(TRANG_THAI_HO_SO, 'CHO_XAC_MINH') TRANG_THAI_HO_SO,
                       NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC
                FROM TB_NHANVIEN_THUE
                WHERE TRANG_THAI = 'ACTIVE'").ToList();

            var dependents = PayrollPreparationSql.Query<DependentRow>(db, @"
                SELECT ID, MANV, HO_TEN, MOI_QUAN_HE, CCCD, MA_SO_THUE_NPT, NGAY_SINH,
                       NVL(LOAI_GIAY_TO, 'CHUA_XAC_MINH') LOAI_GIAY_TO, SO_GIAY_TO, MA_DINH_DANH_CA_NHAN,
                       CAN_CU_GIAM_TRU, NVL(TRANG_THAI_DANG_KY, 'CHO_XAC_MINH') TRANG_THAI_DANG_KY,
                       NVL(TINH_TRANG_HO_SO_CHUNG_MINH, 'CHUA_NOP') TINH_TRANG_HO_SO_CHUNG_MINH,
                       THANG_BAT_DAU_GIAM_TRU, THANG_KET_THUC_GIAM_TRU
                FROM TB_NGUOI_PHU_THUOC
                WHERE TRANG_THAI = 'ACTIVE'").ToList();

            var registrations = PayrollPreparationSql.Query<DependentRegistrationRow>(db, @"
                SELECT ID, ID_NPT, MANV, THANG_BAT_DAU, THANG_KET_THUC, TRANG_THAI
                FROM TB_DANG_KY_GIAM_TRU_NPT
                WHERE TRANG_THAI = 'ACTIVE'").ToList();

            var issues = new List<PayrollPreparationIssue>();
            int totalDependentsApplied = 0;

            foreach (var e in employees)
            {
                var date = ProfileDate(e, end);
                var activeProfiles = taxProfiles.Where(t => t.MANV == e.MANV && t.NGAY_HIEU_LUC <= date && (!t.NGAY_HET_HIEU_LUC.HasValue || t.NGAY_HET_HIEU_LUC >= date)).ToList();

                if (activeProfiles.Count == 0)
                {
                    issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NHANVIEN_THUE", Reason = $"Thiếu hồ sơ thuế hiệu lực ngày {date:dd/MM/yyyy}. Mở Hồ sơ thuế để khai báo." });
                    continue;
                }
                if (activeProfiles.Count > 1)
                {
                    issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NHANVIEN_THUE", Reason = $"Trùng khoảng hiệu lực hồ sơ thuế ({activeProfiles.Count} bản ghi) ngày {date:dd/MM/yyyy}." });
                    continue;
                }

                var profile = activeProfiles[0];

                if (profile.IS_CU_TRU == 0 && profile.CO_UY_QUYEN_QUYET_TOAN == 1)
                {
                    issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NHANVIEN_THUE", Reason = "Cá nhân không cư trú không được ủy quyền quyết toán thuế (chỉ áp dụng thuế suất cố định 20%)." });
                }

                if (profile.LOAI_DINH_DANH == "HO_CHIEU")
                {
                    if (string.IsNullOrWhiteSpace(profile.SO_HO_CHIEU) && string.IsNullOrWhiteSpace(profile.MA_SO_THUE))
                    {
                        issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NHANVIEN_THUE", Reason = "Lao động nước ngoài thiếu số Hộ chiếu / MST để kê khai thuế." });
                    }
                    if (profile.CAN_CU_CU_TRU == "CHUA_XAC_MINH")
                    {
                        issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NHANVIEN_THUE", Reason = "Chưa xác minh căn cứ cư trú lao động nước ngoài (cần xác nhận số ngày có mặt >= 183 ngày hoặc hợp đồng thuê nhà để áp dụng biểu thuế đúng)." });
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(profile.MA_SO_THUE))
                    {
                        issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NHANVIEN_THUE", Reason = "Thiếu mã số thuế / số định danh cá nhân." });
                    }
                }

                var empDeps = dependents.Where(d => d.MANV == e.MANV && d.THANG_BAT_DAU_GIAM_TRU <= period && (!d.THANG_KET_THUC_GIAM_TRU.HasValue || d.THANG_KET_THUC_GIAM_TRU >= period)).ToList();
                totalDependentsApplied += empDeps.Count;

                foreach (var dep in empDeps)
                {
                    if (dep.NGAY_SINH.HasValue && dep.NGAY_SINH.Value.Date > end.Date)
                    {
                        issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NGUOI_PHU_THUOC", Reason = $"Người phụ thuộc {dep.HO_TEN} có ngày sinh ({dep.NGAY_SINH:dd/MM/yyyy}) ở tương lai." });
                    }
                    if (dep.THANG_KET_THUC_GIAM_TRU.HasValue && dep.THANG_KET_THUC_GIAM_TRU.Value < dep.THANG_BAT_DAU_GIAM_TRU)
                    {
                        issues.Add(new PayrollPreparationIssue { EmployeeId = e.MANV, EmployeeName = e.HOTEN, ActionTable = "TB_NGUOI_PHU_THUOC", Reason = $"Người phụ thuộc {dep.HO_TEN} có tháng kết thúc ({dep.THANG_KET_THUC_GIAM_TRU}) trước tháng bắt đầu ({dep.THANG_BAT_DAU_GIAM_TRU})." });
                    }
                }
            }

            var activeDepsInPeriod = dependents.Where(d => d.THANG_BAT_DAU_GIAM_TRU <= period && (!d.THANG_KET_THUC_GIAM_TRU.HasValue || d.THANG_KET_THUC_GIAM_TRU >= period)).ToList();

            for (int i = 0; i < activeDepsInPeriod.Count; i++)
            {
                for (int j = i + 1; j < activeDepsInPeriod.Count; j++)
                {
                    var d1 = activeDepsInPeriod[i];
                    var d2 = activeDepsInPeriod[j];
                    if (d1.MANV == d2.MANV) continue;

                    bool duplicateId = (!string.IsNullOrWhiteSpace(d1.CCCD) && d1.CCCD == d2.CCCD)
                                    || (!string.IsNullOrWhiteSpace(d1.MA_SO_THUE_NPT) && d1.MA_SO_THUE_NPT == d2.MA_SO_THUE_NPT)
                                    || (!string.IsNullOrWhiteSpace(d1.MA_DINH_DANH_CA_NHAN) && d1.MA_DINH_DANH_CA_NHAN == d2.MA_DINH_DANH_CA_NHAN)
                                    || (!string.IsNullOrWhiteSpace(d1.SO_GIAY_TO) && d1.LOAI_GIAY_TO == d2.LOAI_GIAY_TO && d1.SO_GIAY_TO == d2.SO_GIAY_TO);

                    if (duplicateId)
                    {
                        var emp1 = employees.FirstOrDefault(emp => emp.MANV == d1.MANV);
                        issues.Add(new PayrollPreparationIssue {
                            EmployeeId = d1.MANV,
                            EmployeeName = emp1 != null ? emp1.HOTEN : $"NV {d1.MANV}",
                            ActionTable = "TB_NGUOI_PHU_THUOC",
                            Reason = $"Người phụ thuộc {d1.HO_TEN} bị khai đồng thời bởi nhân viên khác (trùng định danh/giấy tờ với NV {d2.MANV})."
                        });
                    }
                    else
                    {
                        string norm1 = NormalizeName(d1.HO_TEN);
                        string norm2 = NormalizeName(d2.HO_TEN);
                        if (!string.IsNullOrEmpty(norm1) && norm1 == norm2 && d1.NGAY_SINH.HasValue && d2.NGAY_SINH.HasValue && d1.NGAY_SINH.Value.Date == d2.NGAY_SINH.Value.Date && d1.MOI_QUAN_HE == d2.MOI_QUAN_HE)
                        {
                            var emp1 = employees.FirstOrDefault(emp => emp.MANV == d1.MANV);
                            issues.Add(new PayrollPreparationIssue {
                                EmployeeId = d1.MANV,
                                EmployeeName = emp1 != null ? emp1.HOTEN : $"NV {d1.MANV}",
                                ActionTable = "TB_NGUOI_PHU_THUOC",
                                Reason = $"Nghi vấn trùng người phụ thuộc {d1.HO_TEN} với nhân viên khác (trùng họ tên, ngày sinh {d1.NGAY_SINH:dd/MM/yyyy}, quan hệ {d1.MOI_QUAN_HE} với NV {d2.MANV}). Cần đối chiếu xác minh."
                            });
                        }
                    }
                }
            }

            var activeRegsInPeriod = registrations.Where(r => r.THANG_BAT_DAU <= period && (!r.THANG_KET_THUC.HasValue || r.THANG_KET_THUC >= period)).ToList();
            var groupedByNpt = activeRegsInPeriod.GroupBy(r => r.ID_NPT).Where(g => g.Select(x => x.MANV).Distinct().Count() > 1);
            foreach (var g in groupedByNpt)
            {
                var first = g.First();
                var emp = employees.FirstOrDefault(x => x.MANV == first.MANV);
                issues.Add(new PayrollPreparationIssue {
                    EmployeeId = first.MANV,
                    EmployeeName = emp != null ? emp.HOTEN : $"NV {first.MANV}",
                    ActionTable = "TB_DANG_KY_GIAM_TRU_NPT",
                    Reason = $"Người phụ thuộc (ID {g.Key}) có nhiều đăng ký giảm trừ chồng lấn cho nhiều nhân viên trong kỳ {period}."
                });
            }

            string guidance = issues.Count == 0
                ? $"Đã kiểm tra. {employees.Count} nhân viên đủ điều kiện tính lương có {totalDependentsApplied} người phụ thuộc áp dụng giảm trừ gia cảnh."
                : $"{issues.Count} mục cần bổ sung/xác minh hồ sơ thuế và người phụ thuộc.";

            Add(report, "6. Hồ sơ thuế và người phụ thuộc", PayrollPreparationTarget.Tax, false, guidance, issues);
        }
        private static void InspectProfiles(MyEntities db,PayrollReadinessReport report,List<EmployeeRow> employees,DateTime end,string table,string from,string to,PayrollPreparationTarget target,string title)
        {
            var profiles=PayrollPreparationSql.Query<ProfileRow>(db,$"SELECT MANV,{from} START_DATE,{to} END_DATE FROM {table} WHERE TRANG_THAI='ACTIVE'").ToList();
            var issues=new List<PayrollPreparationIssue>();
            foreach(var e in employees) { var date=ProfileDate(e,end); var count=EffectiveProfileCount(profiles,e.MANV,date);
                if(count!=1) issues.Add(Issue(e,$"{(count==0 ? "Thiếu" : "Trùng")} hồ sơ hiệu lực ngày {date:dd/MM/yyyy}.")); }
            Add(report,title,target,false,$"{issues.Count} nhân viên cần bổ sung/sửa thời hạn hồ sơ. Khai báo rõ Có/Không; không suy đoán tình trạng tham gia hoặc cư trú.",issues);
        }
        private static void InspectPolicies(MyEntities db,PayrollReadinessReport report,DateTime date)
        {
            var errors=new List<PayrollPreparationIssue>(); decimal? insurance=null,tax=null;
            foreach(var g in new[] {Tuple.Create("TB_CHINH_SACH_LUONG","Lương"),Tuple.Create("TB_CHINH_SACH_BHXH","BHXH"),Tuple.Create("TB_CHINH_SACH_CONG_DOAN","Công đoàn"),Tuple.Create("TB_THUE_TNCN_CHINH_SACH","Thuế TNCN")}) {
                var year=g.Item1=="TB_THUE_TNCN_CHINH_SACH" ? " AND TAX_YEAR="+date.Year : "";
                var rows=PayrollPreparationSql.Query<PolicyRow>(db,$"SELECT ID FROM {g.Item1} WHERE TRANG_THAI='ACTIVE' AND NGAY_HIEU_LUC<=:p0 AND (NGAY_HET_HIEU_LUC IS NULL OR NGAY_HET_HIEU_LUC>=:p0){year}",new OracleParameter("p0",date)).ToList();
                if(rows.Count!=1) errors.Add(new PayrollPreparationIssue { Reason=$"{g.Item2}: cần đúng 1 chính sách hiệu lực ngày {date:dd/MM/yyyy}; hiện có {rows.Count}." });
                else if(g.Item1=="TB_CHINH_SACH_BHXH") insurance=rows[0].ID; else if(g.Item1=="TB_THUE_TNCN_CHINH_SACH") tax=rows[0].ID;
            }
            if(insurance.HasValue) { var regions=PayrollPreparationSql.Query<decimal>(db,"SELECT VUNG_LUONG FROM TB_CHINH_SACH_BHXH_VUNG WHERE POLICY_BHXH_ID=:p0",new OracleParameter("p0",insurance.Value)).ToList();
                if(regions.Count!=4 || !new decimal[] {1,2,3,4}.All(regions.Contains)) errors.Add(new PayrollPreparationIssue {Reason="Chính sách BHXH cần đủ 4 vùng lương tối thiểu."}); }
            if(tax.HasValue) { var rows=PayrollPreparationSql.Query<TaxBracketDto>(db,"SELECT ID,POLICY_THUE_ID,TAX_YEAR,PERIOD_TYPE,BAC_THUE,CAN_DUOI,CAN_TREN,THUE_SUAT FROM TB_THUE_TNCN_BAC WHERE POLICY_THUE_ID=:p0 AND PERIOD_TYPE='MONTH' ORDER BY BAC_THUE",new OracleParameter("p0",tax.Value)).ToList();
                if(!ValidMonthlyBrackets(rows)) errors.Add(new PayrollPreparationIssue {Reason="Biểu thuế tháng cần đủ 5 bậc liên tiếp từ 0 đến không giới hạn; kiểm tra cận và thuế suất."}); }
            Add(report,"2. Chính sách áp dụng cuối kỳ",PayrollPreparationTarget.Policies,errors.Count>0,"Quản lý chính sách lương, BHXH/vùng, công đoàn, giảm trừ và bậc thuế. Dùng phiên bản đúng ngày hiệu lực; không thay thế số liệu lịch sử.",errors);
        }
        public static bool ValidMonthlyBrackets(IList<TaxBracketDto> rows)
        {
            if(rows.Count!=5 || rows[0].CAN_DUOI!=0 || rows[4].CAN_TREN.HasValue) return false;
            for(int i=0;i<rows.Count;i++) if(rows[i].BAC_THUE!=i+1 || rows[i].THUE_SUAT<0 || rows[i].THUE_SUAT>1 ||
                (i<4 && (!rows[i].CAN_TREN.HasValue || rows[i].CAN_TREN<=rows[i].CAN_DUOI || rows[i].CAN_TREN!=rows[i+1].CAN_DUOI))) return false;
            return true;
        }
        private static PayrollPreparationIssue Issue(EmployeeRow e,string reason)=>new PayrollPreparationIssue {EmployeeId=e.MANV,EmployeeName=e.HOTEN,Reason=reason};
        private static void Add(PayrollReadinessReport r,string title,PayrollPreparationTarget target,bool blocked,string text,List<PayrollPreparationIssue> issues=null)=>r.Steps.Add(new PayrollPreparationStep {Title=title,Target=target,Blocking=blocked,Status=blocked?"Cần xử lý":(issues!=null && issues.Count>0 ? "Cần lưu ý" : "Đã kiểm tra"),Guidance=text,Issues=issues??new List<PayrollPreparationIssue>()});
    }
}
