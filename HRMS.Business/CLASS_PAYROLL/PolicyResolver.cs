using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Bu.CLASS_SYSTEM;

namespace Bu.CLASS_PAYROLL
{
    public interface IPolicyResolver
    {
        SalaryPolicyDto GetSalaryPolicy(DateTime effectiveDate);
        InsurancePolicyDto GetInsurancePolicy(DateTime effectiveDate);
        List<InsuranceRegionDto> GetInsuranceRegions(decimal policyBhxhId);
        InsuranceRegionDto GetInsuranceRegion(decimal policyBhxhId, int region);
        UnionPolicyDto GetUnionPolicy(DateTime effectiveDate);
        TaxPolicyDto GetTaxPolicy(int taxYear, DateTime effectiveDate);
        List<TaxBracketDto> GetTaxBrackets(decimal policyThueId, string periodType);
        void RefreshCache();
    }

    public class PolicyResolver : IPolicyResolver
    {
        private static readonly object _syncLock = new object();
        private static List<SalaryPolicyDto> _salaryPolicies;
        private static List<InsurancePolicyDto> _insurancePolicies;
        private static List<InsuranceRegionDto> _insuranceRegions;
        private static List<UnionPolicyDto> _unionPolicies;
        private static List<TaxPolicyDto> _taxPolicies;
        private static List<TaxBracketDto> _taxBrackets;

        public PolicyResolver()
        {
            EnsureLoaded();
        }

        public void RefreshCache()
        {
            lock (_syncLock)
            {
                _salaryPolicies = null;
                _insurancePolicies = null;
                _insuranceRegions = null;
                _unionPolicies = null;
                _taxPolicies = null;
                _taxBrackets = null;
                EnsureLoaded();
            }
        }

        public static List<PolicyGroupInspectionResult> InspectAllPolicyGroups(DateTime? effectiveDate = null)
        {
            DateTime effDate = effectiveDate ?? DateTime.Today;
            var results = new List<PolicyGroupInspectionResult>
            {
                InspectSalaryGroup(effDate),
                InspectInsuranceGroup(effDate),
                InspectUnionGroup(effDate),
                InspectTaxGroup(effDate)
            };
            return results;
        }

        public static bool IsPolicySchemaAvailable(out string missingDetails)
        {
            var inspections = InspectAllPolicyGroups(DateTime.Today);
            var notReady = inspections.Where(i => i.Status != PolicyGroupStatus.Ready).ToList();
            if (notReady.Count == 0)
            {
                missingDetails = null;
                return true;
            }

            missingDetails = string.Join("; ", notReady.Select(n => $"{n.GroupName} ({n.PrimaryTable}): {n.StatusDisplay}"));
            return false;
        }

        private static PolicyGroupInspectionResult InspectSalaryGroup(DateTime effDate)
        {
            string correlationId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var res = new PolicyGroupInspectionResult
            {
                GroupKey = "SALARY",
                GroupName = "Chính sách tham số lương chuẩn",
                PrimaryTable = "TB_CHINH_SACH_LUONG",
                CorrelationId = correlationId
            };

            try
            {
                using (var db = new MyEntities())
                {
                    var policies = db.Database.SqlQuery<SalaryPolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                               SO_CONG_CHUAN_THANG, SO_GIO_CHUAN_NGAY, HE_SO_LAM_DEM, TRANG_THAI
                        FROM TB_CHINH_SACH_LUONG
                        ORDER BY NGAY_HIEU_LUC DESC").ToList();

                    if (policies.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoPolicyRecord;
                        res.StatusDisplay = "[Chưa có dữ liệu] Bảng TB_CHINH_SACH_LUONG tồn tại nhưng chưa có bản ghi chính sách";
                        res.ActionRequired = "Cần nạp dữ liệu chính sách lương áp dụng vào bảng TB_CHINH_SACH_LUONG";
                        return res;
                    }

                    var activeMatches = policies.Where(p => string.Equals(p.TRANG_THAI, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
                                                            p.NGAY_HIEU_LUC <= effDate &&
                                                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effDate)).ToList();

                    if (activeMatches.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoEffectivePolicy;
                        res.StatusDisplay = $"[Hết hiệu lực] Có {policies.Count} bản ghi nhưng không có chính sách nào ACTIVE phù hợp ngày {effDate:dd/MM/yyyy}";
                        res.ActionRequired = "Cần cập nhật hoặc ban hành chính sách lương mới có hiệu lực";
                        return res;
                    }

                    var sal = activeMatches.First();
                    res.Status = PolicyGroupStatus.Ready;
                    res.ValueSummary = $"Công chuẩn: {sal.SO_CONG_CHUAN_THANG} ngày | Giờ chuẩn: {sal.SO_GIO_CHUAN_NGAY}h | Ca đêm: {sal.HE_SO_LAM_DEM * 100}%";
                    res.EffectivePeriod = $"Từ {sal.NGAY_HIEU_LUC:dd/MM/yyyy}";
                    res.LegalReference = sal.TEN_CHINH_SACH ?? "Quy chế lương công ty";
                    return res;
                }
            }
            catch (Exception ex)
            {
                MapExceptionToInspectionResult(ex, res, "TB_CHINH_SACH_LUONG", correlationId);
                return res;
            }
        }

        private static PolicyGroupInspectionResult InspectInsuranceGroup(DateTime effDate)
        {
            string correlationId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var res = new PolicyGroupInspectionResult
            {
                GroupKey = "INSURANCE",
                GroupName = "Chính sách bảo hiểm xã hội (BHXH)",
                PrimaryTable = "TB_CHINH_SACH_BHXH",
                DependentTables = new List<string> { "TB_CHINH_SACH_BHXH_VUNG" },
                CorrelationId = correlationId
            };

            try
            {
                using (var db = new MyEntities())
                {
                    var policies = db.Database.SqlQuery<InsurancePolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, MUC_THAM_CHIEU,
                               TY_LE_BHXH_NLD, TY_LE_BHYT_NLD, TY_LE_BHTN_NLD,
                               TY_LE_BHXH_NSDLD, TY_LE_BHYT_NSDLD, TY_LE_BHTN_NSDLD,
                               TY_LE_TNLD_BNN_NSDLD, TY_LE_TNLD_BNN_UU_DAI,
                               AP_DUNG_TRAN_BHXH_BHYT, AP_DUNG_TRAN_BHTN, TRANG_THAI
                        FROM TB_CHINH_SACH_BHXH
                        ORDER BY NGAY_HIEU_LUC DESC").ToList();

                    if (policies.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoPolicyRecord;
                        res.StatusDisplay = "[Chưa có dữ liệu] Bảng TB_CHINH_SACH_BHXH tồn tại nhưng chưa có bản ghi chính sách";
                        res.ActionRequired = "Cần nạp dữ liệu chính sách BHXH vào bảng TB_CHINH_SACH_BHXH";
                        return res;
                    }

                    var activeMatches = policies.Where(p => string.Equals(p.TRANG_THAI, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
                                                            p.NGAY_HIEU_LUC <= effDate &&
                                                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effDate)).ToList();

                    if (activeMatches.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoEffectivePolicy;
                        res.StatusDisplay = $"[Hết hiệu lực] Có {policies.Count} bản ghi nhưng không có chính sách BHXH nào ACTIVE phù hợp ngày {effDate:dd/MM/yyyy}";
                        res.ActionRequired = "Cần cập nhật hoặc ban hành chính sách BHXH mới có hiệu lực";
                        return res;
                    }

                    var ins = activeMatches.First();

                    // Kiểm tra bảng phụ thuộc vùng lương
                    try
                    {
                        var regions = db.Database.SqlQuery<InsuranceRegionDto>(
                            $"SELECT ID, POLICY_BHXH_ID, VUNG_LUONG, LUONG_TOI_THIEU_THANG, LUONG_TOI_THIEU_GIO, HE_SO_SAN_DOANH_NGHIEP FROM TB_CHINH_SACH_BHXH_VUNG WHERE POLICY_BHXH_ID = {ins.ID}"
                        ).ToList();
                        if (regions.Count == 0)
                        {
                            res.Status = PolicyGroupStatus.NoPolicyRecord;
                            res.StatusDisplay = $"[Thiếu vùng lương] Chưa có bản ghi vùng lương trong TB_CHINH_SACH_BHXH_VUNG cho chính sách {ins.MA_CHINH_SACH}";
                            res.ActionRequired = "Cần cấu hình 4 vùng lương tối thiểu cho chính sách BHXH";
                            return res;
                        }
                    }
                    catch (Exception regEx)
                    {
                        MapExceptionToInspectionResult(regEx, res, "TB_CHINH_SACH_BHXH_VUNG", correlationId);
                        return res;
                    }

                    res.Status = PolicyGroupStatus.Ready;
                    res.ValueSummary = $"Lương tham chiếu: {ins.MUC_THAM_CHIEU:N0} đ (NLĐ: {(ins.TY_LE_BHXH_NLD + ins.TY_LE_BHYT_NLD + ins.TY_LE_BHTN_NLD) * 100}%)";
                    res.EffectivePeriod = $"Từ {ins.NGAY_HIEU_LUC:dd/MM/yyyy}";
                    res.LegalReference = ins.TEN_CHINH_SACH ?? "Nghị định 73/2024/NĐ-CP & Luật BHXH";
                    return res;
                }
            }
            catch (Exception ex)
            {
                MapExceptionToInspectionResult(ex, res, "TB_CHINH_SACH_BHXH", correlationId);
                return res;
            }
        }

        private static PolicyGroupInspectionResult InspectUnionGroup(DateTime effDate)
        {
            string correlationId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var res = new PolicyGroupInspectionResult
            {
                GroupKey = "UNION",
                GroupName = "Chính sách tài chính công đoàn",
                PrimaryTable = "TB_CHINH_SACH_CONG_DOAN",
                CorrelationId = correlationId
            };

            try
            {
                using (var db = new MyEntities())
                {
                    var policies = db.Database.SqlQuery<UnionPolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                               TY_LE_DOAN_PHI_NLD, CAP_PERCENT_STATUTORY_BASE_SALARY, KINH_PHI_CONG_DOAN_NSDLD, TRANG_THAI
                        FROM TB_CHINH_SACH_CONG_DOAN
                        ORDER BY NGAY_HIEU_LUC DESC").ToList();

                    if (policies.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoPolicyRecord;
                        res.StatusDisplay = "[Chưa có dữ liệu] Bảng TB_CHINH_SACH_CONG_DOAN tồn tại nhưng chưa có bản ghi chính sách";
                        res.ActionRequired = "Cần nạp dữ liệu chính sách công đoàn vào bảng TB_CHINH_SACH_CONG_DOAN";
                        return res;
                    }

                    var activeMatches = policies.Where(p => string.Equals(p.TRANG_THAI, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
                                                            p.NGAY_HIEU_LUC <= effDate &&
                                                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effDate)).ToList();

                    if (activeMatches.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoEffectivePolicy;
                        res.StatusDisplay = $"[Hết hiệu lực] Có {policies.Count} bản ghi nhưng không có chính sách công đoàn nào ACTIVE phù hợp ngày {effDate:dd/MM/yyyy}";
                        res.ActionRequired = "Cần cập nhật hoặc ban hành chính sách công đoàn mới có hiệu lực";
                        return res;
                    }

                    var un = activeMatches.First();
                    res.Status = PolicyGroupStatus.Ready;
                    res.ValueSummary = $"Đoàn phí NLĐ: {un.TY_LE_DOAN_PHI_NLD * 100}% | NSDLĐ đóng: {un.KINH_PHI_CONG_DOAN_NSDLD * 100}%";
                    res.EffectivePeriod = $"Từ {un.NGAY_HIEU_LUC:dd/MM/yyyy}";
                    res.LegalReference = un.TEN_CHINH_SACH ?? "Quyết định 61/QĐ-TLĐ & Nghị định 105/2026/NĐ-CP";
                    return res;
                }
            }
            catch (Exception ex)
            {
                MapExceptionToInspectionResult(ex, res, "TB_CHINH_SACH_CONG_DOAN", correlationId);
                return res;
            }
        }

        private static PolicyGroupInspectionResult InspectTaxGroup(DateTime effDate)
        {
            string correlationId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            var res = new PolicyGroupInspectionResult
            {
                GroupKey = "TAX",
                GroupName = "Chính sách thuế thu nhập cá nhân (TNCN)",
                PrimaryTable = "TB_THUE_TNCN_CHINH_SACH",
                DependentTables = new List<string> { "TB_THUE_TNCN_BAC" },
                CorrelationId = correlationId
            };

            try
            {
                using (var db = new MyEntities())
                {
                    var policies = db.Database.SqlQuery<TaxPolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, TAX_YEAR, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                               GIAM_TRU_BAN_THAN_THANG, GIAM_TRU_PHU_THUOC_THANG,
                               GIAM_TRU_BAN_THAN_NAM, GIAM_TRU_PHU_THUOC_NAM, TRANG_THAI
                        FROM TB_THUE_TNCN_CHINH_SACH
                        ORDER BY NGAY_HIEU_LUC DESC").ToList();

                    if (policies.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoPolicyRecord;
                        res.StatusDisplay = "[Chưa có dữ liệu] Bảng TB_THUE_TNCN_CHINH_SACH tồn tại nhưng chưa có bản ghi chính sách";
                        res.ActionRequired = "Cần nạp dữ liệu chính sách thuế TNCN vào bảng TB_THUE_TNCN_CHINH_SACH";
                        return res;
                    }

                    int taxYear = effDate.Year;
                    var activeMatches = policies.Where(p => string.Equals(p.TRANG_THAI, "ACTIVE", StringComparison.OrdinalIgnoreCase) &&
                                                            p.TAX_YEAR == taxYear &&
                                                            p.NGAY_HIEU_LUC <= effDate &&
                                                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effDate)).ToList();

                    if (activeMatches.Count == 0)
                    {
                        res.Status = PolicyGroupStatus.NoEffectivePolicy;
                        res.StatusDisplay = $"[Hết hiệu lực] Có {policies.Count} bản ghi nhưng không có chính sách thuế nào ACTIVE cho năm {taxYear} tại {effDate:dd/MM/yyyy}";
                        res.ActionRequired = $"Cần cấu hình chính sách thuế TNCN cho năm {taxYear}";
                        return res;
                    }

                    var tax = activeMatches.First();

                    // Kiểm tra bảng bậc thuế TNCN
                    try
                    {
                        var brackets = db.Database.SqlQuery<TaxBracketDto>(
                            $"SELECT ID, POLICY_THUE_ID, TAX_YEAR, PERIOD_TYPE, BAC_THUE, CAN_DUOI, CAN_TREN, THUE_SUAT FROM TB_THUE_TNCN_BAC WHERE POLICY_THUE_ID = {tax.ID}"
                        ).ToList();
                        if (brackets.Count == 0)
                        {
                            res.Status = PolicyGroupStatus.NoPolicyRecord;
                            res.StatusDisplay = $"[Thiếu bậc thuế] Chưa có các bậc thuế TNCN trong TB_THUE_TNCN_BAC cho chính sách {tax.MA_CHINH_SACH}";
                            res.ActionRequired = "Cần cấu hình các bậc thuế TNCN biểu lũy tiến";
                            return res;
                        }
                    }
                    catch (Exception brEx)
                    {
                        MapExceptionToInspectionResult(brEx, res, "TB_THUE_TNCN_BAC", correlationId);
                        return res;
                    }

                    res.Status = PolicyGroupStatus.Ready;
                    res.ValueSummary = $"Giảm trừ bản thân: {tax.GIAM_TRU_BAN_THAN_THANG:N0} đ/th | Phụ thuộc: {tax.GIAM_TRU_PHU_THUOC_THANG:N0} đ/người/th";
                    res.EffectivePeriod = $"Năm thuế {tax.TAX_YEAR} (từ {tax.NGAY_HIEU_LUC:dd/MM/yyyy})";
                    res.LegalReference = tax.TEN_CHINH_SACH ?? "Luật Thuế TNCN & Nghị quyết 954/2020";
                    return res;
                }
            }
            catch (Exception ex)
            {
                MapExceptionToInspectionResult(ex, res, "TB_THUE_TNCN_CHINH_SACH", correlationId);
                return res;
            }
        }

        private static void MapExceptionToInspectionResult(Exception ex, PolicyGroupInspectionResult res, string tableName, string correlationId)
        {
            string exText = ex.ToString();
            res.TechnicalError = ex.Message;
            System.Diagnostics.Trace.TraceWarning($"[{correlationId}] InspectPolicyGroup ({tableName}) failed: {ex}");

            if (exText.IndexOf("ORA-00942", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exText.IndexOf("table or view does not exist", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                res.Status = PolicyGroupStatus.ObjectNotFound;
                res.StatusDisplay = $"[Chưa sẵn sàng] Bảng '{tableName}' chưa tồn tại trong CSDL hoặc thiếu quyền truy cập (ORA-00942)";
                res.ActionRequired = "Yêu cầu Quản trị viên (DBA) thực thi migration 'apply_payroll_v1_16_objects.sql' trên schema CSDL";
            }
            else if (exText.IndexOf("ORA-00904", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     exText.IndexOf("invalid identifier", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                res.Status = PolicyGroupStatus.ColumnMismatch;
                res.StatusDisplay = $"[Chưa tương thích] Bảng '{tableName}' thiếu cột cấu trúc hoặc sai kiểu dữ liệu (ORA-00904)";
                res.ActionRequired = $"Yêu cầu Quản trị viên (DBA) nâng cấp cấu trúc bảng '{tableName}'";
            }
            else if (exText.IndexOf("ORA-12541", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     exText.IndexOf("ORA-12154", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     exText.IndexOf("ORA-12170", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     exText.IndexOf("ORA-50000", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     ex.GetType().Name.Contains("SocketException"))
            {
                res.Status = PolicyGroupStatus.ConnectionFailed;
                res.StatusDisplay = "[Lỗi kết nối] Không thể kết nối tới cơ sở dữ liệu Oracle";
                res.ActionRequired = "Kiểm tra đường truyền mạng và dịch vụ Oracle Database";
            }
            else
            {
                res.Status = PolicyGroupStatus.QueryFailed;
                res.StatusDisplay = $"[Lỗi truy vấn] Sự cố khi đọc bảng '{tableName}' (Mã: {correlationId})";
                res.ActionRequired = "Kiểm tra nhật ký hệ thống";
            }
        }

        private void EnsureLoaded()
        {
            if (_salaryPolicies != null) return;

            lock (_syncLock)
            {
                if (_salaryPolicies != null) return;

                using (var db = new MyEntities())
                {
                    string currentStep = "Khởi tạo kết nối";
                    string currentTable = "TB_CHINH_SACH_LUONG";
                    string refId = "REF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                    try
                    {
                        currentStep = "Tải chính sách lương chuẩn";
                        currentTable = "TB_CHINH_SACH_LUONG";
                        _salaryPolicies = db.Database.SqlQuery<SalaryPolicyDto>(@"
                            SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                                   SO_CONG_CHUAN_THANG, SO_GIO_CHUAN_NGAY, HE_SO_LAM_DEM, TRANG_THAI
                            FROM TB_CHINH_SACH_LUONG
                            WHERE TRANG_THAI = 'ACTIVE'
                            ORDER BY NGAY_HIEU_LUC").ToList();

                        currentStep = "Tải chính sách bảo hiểm xã hội";
                        currentTable = "TB_CHINH_SACH_BHXH";
                        _insurancePolicies = db.Database.SqlQuery<InsurancePolicyDto>(@"
                            SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, MUC_THAM_CHIEU,
                                   TY_LE_BHXH_NLD, TY_LE_BHYT_NLD, TY_LE_BHTN_NLD,
                                   TY_LE_BHXH_NSDLD, TY_LE_BHYT_NSDLD, TY_LE_BHTN_NSDLD,
                                   TY_LE_TNLD_BNN_NSDLD, TY_LE_TNLD_BNN_UU_DAI,
                                   AP_DUNG_TRAN_BHXH_BHYT, AP_DUNG_TRAN_BHTN, TRANG_THAI
                            FROM TB_CHINH_SACH_BHXH
                            WHERE TRANG_THAI = 'ACTIVE'
                            ORDER BY NGAY_HIEU_LUC").ToList();

                        currentStep = "Tải bảng lương tối thiểu vùng BHXH";
                        currentTable = "TB_CHINH_SACH_BHXH_VUNG";
                        _insuranceRegions = db.Database.SqlQuery<InsuranceRegionDto>(@"
                            SELECT ID, POLICY_BHXH_ID, VUNG_LUONG, LUONG_TOI_THIEU_THANG, LUONG_TOI_THIEU_GIO, HE_SO_SAN_DOANH_NGHIEP
                            FROM TB_CHINH_SACH_BHXH_VUNG
                            ORDER BY POLICY_BHXH_ID, VUNG_LUONG").ToList();

                        currentStep = "Tải chính sách kinh phí công đoàn";
                        currentTable = "TB_CHINH_SACH_CONG_DOAN";
                        _unionPolicies = db.Database.SqlQuery<UnionPolicyDto>(@"
                            SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                                   TY_LE_DOAN_PHI_NLD, CAP_PERCENT_STATUTORY_BASE_SALARY, KINH_PHI_CONG_DOAN_NSDLD, TRANG_THAI
                            FROM TB_CHINH_SACH_CONG_DOAN
                            WHERE TRANG_THAI = 'ACTIVE'
                            ORDER BY NGAY_HIEU_LUC").ToList();

                        currentStep = "Tải chính sách giảm trừ thuế TNCN";
                        currentTable = "TB_THUE_TNCN_CHINH_SACH";
                        _taxPolicies = db.Database.SqlQuery<TaxPolicyDto>(@"
                            SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, TAX_YEAR, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                                   GIAM_TRU_BAN_THAN_THANG, GIAM_TRU_PHU_THUOC_THANG,
                                   GIAM_TRU_BAN_THAN_NAM, GIAM_TRU_PHU_THUOC_NAM, TRANG_THAI
                            FROM TB_THUE_TNCN_CHINH_SACH
                            WHERE TRANG_THAI = 'ACTIVE'
                            ORDER BY NGAY_HIEU_LUC").ToList();

                        currentStep = "Tải biểu bậc thuế suất lũy tiến TNCN";
                        currentTable = "TB_THUE_TNCN_BAC";
                        _taxBrackets = db.Database.SqlQuery<TaxBracketDto>(@"
                            SELECT ID, POLICY_THUE_ID, TAX_YEAR, PERIOD_TYPE, BAC_THUE, CAN_DUOI, CAN_TREN, THUE_SUAT
                            FROM TB_THUE_TNCN_BAC
                            ORDER BY POLICY_THUE_ID, PERIOD_TYPE, BAC_THUE").ToList();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError($"[{refId}] Lỗi tại bước '{currentStep}' (Bảng '{currentTable}'): {ex}");
                        string exText = ex.ToString();
                        if (exText.IndexOf("ORA-00942", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            exText.IndexOf("table or view does not exist", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new PolicySchemaMissingException(currentTable, "apply_payroll_v1_16_objects.sql", ex);
                        }
                        if (exText.IndexOf("ORA-00904", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            exText.IndexOf("invalid identifier", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            throw new BusinessException("SCHEMA_COLUMN_MISMATCH", $"Bảng '{currentTable}' thiếu cột hoặc không tương thích phiên bản (Mã đối chiếu: {refId}).", null, ex);
                        }
                        throw;
                    }
                }
            }
        }

        public SalaryPolicyDto GetSalaryPolicy(DateTime effectiveDate)
        {
            EnsureLoaded();
            var matches = _salaryPolicies
                .Where(p => p.NGAY_HIEU_LUC <= effectiveDate &&
                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effectiveDate))
                .ToList();

            if (matches.Count == 0)
                throw new PolicyNotFoundException("SALARY", effectiveDate);
            if (matches.Count > 1)
                throw new AmbiguousPolicyException("SALARY", effectiveDate, matches.Count);

            return matches[0];
        }

        public InsurancePolicyDto GetInsurancePolicy(DateTime effectiveDate)
        {
            EnsureLoaded();
            var matches = _insurancePolicies
                .Where(p => p.NGAY_HIEU_LUC <= effectiveDate &&
                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effectiveDate))
                .ToList();

            if (matches.Count == 0)
                throw new PolicyNotFoundException("INSURANCE", effectiveDate);
            if (matches.Count > 1)
                throw new AmbiguousPolicyException("INSURANCE", effectiveDate, matches.Count);

            return matches[0];
        }

        public List<InsuranceRegionDto> GetInsuranceRegions(decimal policyBhxhId)
        {
            EnsureLoaded();
            var regions = _insuranceRegions.Where(r => r.POLICY_BHXH_ID == policyBhxhId).OrderBy(r => r.VUNG_LUONG).ToList();
            if (regions.Count != 4)
                throw new InvalidOperationException($"Expected 4 regional minimum wage definitions for BHXH policy {policyBhxhId}, found {regions.Count}.");
            return regions;
        }

        public InsuranceRegionDto GetInsuranceRegion(decimal policyBhxhId, int region)
        {
            EnsureLoaded();
            var reg = _insuranceRegions.FirstOrDefault(r => r.POLICY_BHXH_ID == policyBhxhId && r.VUNG_LUONG == region);
            if (reg == null)
                throw new PolicyNotFoundException($"INSURANCE_REGION_{region}", DateTime.Today);
            return reg;
        }

        public UnionPolicyDto GetUnionPolicy(DateTime effectiveDate)
        {
            EnsureLoaded();
            var matches = _unionPolicies
                .Where(p => p.NGAY_HIEU_LUC <= effectiveDate &&
                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effectiveDate))
                .ToList();

            if (matches.Count == 0)
                throw new PolicyNotFoundException("UNION", effectiveDate);
            if (matches.Count > 1)
                throw new AmbiguousPolicyException("UNION", effectiveDate, matches.Count);

            return matches[0];
        }

        public TaxPolicyDto GetTaxPolicy(int taxYear, DateTime effectiveDate)
        {
            EnsureLoaded();
            var matches = _taxPolicies
                .Where(p => p.TAX_YEAR == taxYear &&
                            p.NGAY_HIEU_LUC <= effectiveDate &&
                            (!p.NGAY_HET_HIEU_LUC.HasValue || p.NGAY_HET_HIEU_LUC.Value >= effectiveDate))
                .ToList();

            if (matches.Count == 0)
                throw new PolicyNotFoundException($"TAX_{taxYear}", effectiveDate);
            if (matches.Count > 1)
                throw new AmbiguousPolicyException($"TAX_{taxYear}", effectiveDate, matches.Count);

            return matches[0];
        }

        public List<TaxBracketDto> GetTaxBrackets(decimal policyThueId, string periodType)
        {
            EnsureLoaded();
            string upperPeriod = (periodType ?? "MONTH").Trim().ToUpperInvariant();
            var brackets = _taxBrackets
                .Where(b => b.POLICY_THUE_ID == policyThueId && b.PERIOD_TYPE == upperPeriod)
                .OrderBy(b => b.BAC_THUE)
                .ToList();

            if (brackets.Count != 5)
                throw new InvalidOperationException($"Expected 5 tax brackets for policy {policyThueId} ({upperPeriod}), found {brackets.Count}.");

            // Validate bracket contiguousness
            for (int i = 0; i < brackets.Count - 1; i++)
            {
                if (!brackets[i].CAN_TREN.HasValue || brackets[i].CAN_TREN.Value != brackets[i + 1].CAN_DUOI)
                {
                    throw new InvalidOperationException($"Tax bracket continuity gap detected between bracket {brackets[i].BAC_THUE} and {brackets[i + 1].BAC_THUE}.");
                }
            }

            return brackets;
        }
    }
}
