using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

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

        private void EnsureLoaded()
        {
            if (_salaryPolicies != null) return;

            lock (_syncLock)
            {
                if (_salaryPolicies != null) return;

                using (var db = new MyEntities())
                {
                    _salaryPolicies = db.Database.SqlQuery<SalaryPolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                               SO_CONG_CHUAN_THANG, SO_GIO_CHUAN_NGAY, HE_SO_LAM_DEM, TRANG_THAI
                        FROM TB_CHINH_SACH_LUONG
                        WHERE TRANG_THAI = 'ACTIVE'
                        ORDER BY NGAY_HIEU_LUC").ToList();

                    _insurancePolicies = db.Database.SqlQuery<InsurancePolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, MUC_THAM_CHIEU,
                               TY_LE_BHXH_NLD, TY_LE_BHYT_NLD, TY_LE_BHTN_NLD,
                               TY_LE_BHXH_NSDLD, TY_LE_BHYT_NSDLD, TY_LE_BHTN_NSDLD,
                               TY_LE_TNLD_BNN_NSDLD, TY_LE_TNLD_BNN_UU_DAI,
                               AP_DUNG_TRAN_BHXH_BHYT, AP_DUNG_TRAN_BHTN, TRANG_THAI
                        FROM TB_CHINH_SACH_BHXH
                        WHERE TRANG_THAI = 'ACTIVE'
                        ORDER BY NGAY_HIEU_LUC").ToList();

                    _insuranceRegions = db.Database.SqlQuery<InsuranceRegionDto>(@"
                        SELECT ID, POLICY_BHXH_ID, VUNG_LUONG, LUONG_TOI_THIEU_THANG, LUONG_TOI_THIEU_GIO, HE_SO_SAN_DOANH_NGHIEP
                        FROM TB_CHINH_SACH_BHXH_VUNG
                        ORDER BY POLICY_BHXH_ID, VUNG_LUONG").ToList();

                    _unionPolicies = db.Database.SqlQuery<UnionPolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                               TY_LE_DOAN_PHI_NLD, CAP_PERCENT_STATUTORY_BASE_SALARY, KINH_PHI_CONG_DOAN_NSDLD, TRANG_THAI
                        FROM TB_CHINH_SACH_CONG_DOAN
                        WHERE TRANG_THAI = 'ACTIVE'
                        ORDER BY NGAY_HIEU_LUC").ToList();

                    _taxPolicies = db.Database.SqlQuery<TaxPolicyDto>(@"
                        SELECT ID, MA_CHINH_SACH, TEN_CHINH_SACH, TAX_YEAR, NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC,
                               GIAM_TRU_BAN_THAN_THANG, GIAM_TRU_PHU_THUOC_THANG,
                               GIAM_TRU_BAN_THAN_NAM, GIAM_TRU_PHU_THUOC_NAM, TRANG_THAI
                        FROM TB_THUE_TNCN_CHINH_SACH
                        WHERE TRANG_THAI = 'ACTIVE'
                        ORDER BY NGAY_HIEU_LUC").ToList();

                    _taxBrackets = db.Database.SqlQuery<TaxBracketDto>(@"
                        SELECT ID, POLICY_THUE_ID, TAX_YEAR, PERIOD_TYPE, BAC_THUE, CAN_DUOI, CAN_TREN, THUE_SUAT
                        FROM TB_THUE_TNCN_BAC
                        ORDER BY POLICY_THUE_ID, PERIOD_TYPE, BAC_THUE").ToList();
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
