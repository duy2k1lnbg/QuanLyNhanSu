using System;

namespace Bu.CLASS_PAYROLL
{
    public interface IInsuranceEngine
    {
        PayrollInsuranceTraceDto CalculateInsurance(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal contractualBaseSalary,
            InsurancePolicyDto policy,
            InsuranceRegionDto region,
            EmployeeInsuranceProfileDto profile
        );
    }

    public class InsuranceEngine : IInsuranceEngine
    {
        public PayrollInsuranceTraceDto CalculateInsurance(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal contractualBaseSalary,
            InsurancePolicyDto policy,
            InsuranceRegionDto region,
            EmployeeInsuranceProfileDto profile)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (region == null) throw new ArgumentNullException(nameof(region));
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            decimal rawBase = profile.LUONG_DONG_BHXH_RIENG ?? contractualBaseSalary;

            // Regional minimum wage floor (Law on Social Insurance 2024 Art 31)
            decimal floor = region.LUONG_TOI_THIEU_THANG * region.HE_SO_SAN_DOANH_NGHIEP;

            // Statutory caps: 20x reference salary for BHXH/BHYT, 20x regional minimum for BHTN
            decimal capBhxh = policy.AP_DUNG_TRAN_BHXH_BHYT == 1 ? 20.0m * policy.MUC_THAM_CHIEU : decimal.MaxValue;
            decimal capBhtn = policy.AP_DUNG_TRAN_BHTN == 1 ? 20.0m * region.LUONG_TOI_THIEU_THANG : decimal.MaxValue;

            decimal appliedBaseBhxh = Math.Min(Math.Max(rawBase, floor), capBhxh);
            decimal appliedBaseBhtn = Math.Min(Math.Max(rawBase, floor), capBhtn);

            // Employee Deductions
            decimal tienBhxhNld = profile.THAM_GIA_BHXH == 1 ? Math.Round(appliedBaseBhxh * policy.TY_LE_BHXH_NLD, 2) : 0m;
            decimal tienBhytNld = profile.THAM_GIA_BHYT == 1 ? Math.Round(appliedBaseBhxh * policy.TY_LE_BHYT_NLD, 2) : 0m;
            decimal tienBhtnNld = profile.THAM_GIA_BHTN == 1 ? Math.Round(appliedBaseBhtn * policy.TY_LE_BHTN_NLD, 2) : 0m;
            decimal tongBhNld = tienBhxhNld + tienBhytNld + tienBhtnNld;

            // Employer Contributions (Decree 158/2025 & Decree 58/2020)
            decimal tienBhxhNsdld = profile.THAM_GIA_BHXH == 1 ? Math.Round(appliedBaseBhxh * policy.TY_LE_BHXH_NSDLD, 2) : 0m;
            decimal tienBhytNsdld = profile.THAM_GIA_BHYT == 1 ? Math.Round(appliedBaseBhxh * policy.TY_LE_BHYT_NSDLD, 2) : 0m;
            decimal tienBhtnNsdld = profile.THAM_GIA_BHTN == 1 ? Math.Round(appliedBaseBhtn * policy.TY_LE_BHTN_NSDLD, 2) : 0m;

            decimal tyLeTnld = profile.HUONG_TY_LE_TNLD_UU_DAI == 1 ? policy.TY_LE_TNLD_BNN_UU_DAI : policy.TY_LE_TNLD_BNN_NSDLD;
            decimal tienTnldNsdld = profile.THAM_GIA_TNLD_BNN == 1 ? Math.Round(appliedBaseBhxh * tyLeTnld, 2) : 0m;
            decimal tongBhNsdld = tienBhxhNsdld + tienBhytNsdld + tienBhtnNsdld + tienTnldNsdld;

            return new PayrollInsuranceTraceDto
            {
                IDBL = idbl,
                MANV = manv,
                MAKYCONG = makycong,
                POLICY_BHXH_ID = policy.ID,
                PROFILE_BH_ID = profile.ID,
                MUC_THAM_CHIEU = policy.MUC_THAM_CHIEU,
                VUNG_LUONG = region.VUNG_LUONG,
                LUONG_TOI_THIEU_VUNG = region.LUONG_TOI_THIEU_THANG,
                LUONG_DONG_BHXH_GOC = rawBase,
                LUONG_DONG_BHXH_AP_DUNG = appliedBaseBhxh,
                LUONG_DONG_BHTN_AP_DUNG = appliedBaseBhtn,
                TY_LE_BHXH_NLD = policy.TY_LE_BHXH_NLD,
                TIEN_BHXH_NLD = tienBhxhNld,
                TY_LE_BHYT_NLD = policy.TY_LE_BHYT_NLD,
                TIEN_BHYT_NLD = tienBhytNld,
                TY_LE_BHTN_NLD = policy.TY_LE_BHTN_NLD,
                TIEN_BHTN_NLD = tienBhtnNld,
                TONG_BH_NLD = tongBhNld,
                TY_LE_BHXH_NSDLD = policy.TY_LE_BHXH_NSDLD,
                TIEN_BHXH_NSDLD = tienBhxhNsdld,
                TY_LE_BHYT_NSDLD = policy.TY_LE_BHYT_NSDLD,
                TIEN_BHYT_NSDLD = tienBhytNsdld,
                TY_LE_BHTN_NSDLD = policy.TY_LE_BHTN_NSDLD,
                TIEN_BHTN_NSDLD = tienBhtnNsdld,
                TY_LE_TNLD_BNN_NSDLD = tyLeTnld,
                TIEN_TNLD_BNN_NSDLD = tienTnldNsdld,
                TONG_BH_NSDLD = tongBhNsdld
            };
        }
    }
}
