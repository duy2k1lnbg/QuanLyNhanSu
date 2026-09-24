using System;

namespace Bu.CLASS_PAYROLL
{
    public interface IUnionEngine
    {
        PayrollUnionTraceDto CalculateUnion(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal insuranceBaseSalary,
            UnionPolicyDto policy,
            EmployeeUnionProfileDto profile,
            decimal statutoryBaseSalary
        );
    }

    public class UnionEngine : IUnionEngine
    {
        public PayrollUnionTraceDto CalculateUnion(
            decimal idbl,
            decimal manv,
            decimal makycong,
            decimal insuranceBaseSalary,
            UnionPolicyDto policy,
            EmployeeUnionProfileDto profile,
            decimal statutoryBaseSalary)
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            // Decision 61/QD-TLD 2025: 0.5% capped at 10% statutory base salary
            decimal maxCap = Math.Round(policy.CAP_PERCENT_STATUTORY_BASE_SALARY * statutoryBaseSalary, 2);

            decimal tienDoanPhiNld = 0m;
            if (profile.LA_DOAN_VIEN == 1)
            {
                decimal rawDoanPhi = Math.Round(insuranceBaseSalary * policy.TY_LE_DOAN_PHI_NLD, 2);
                tienDoanPhiNld = Math.Min(rawDoanPhi, maxCap);
            }

            // Decree 105/2026/ND-CP: 2% employer contribution on total social insurance payroll base
            decimal tienKpcdNsdld = Math.Round(insuranceBaseSalary * policy.KINH_PHI_CONG_DOAN_NSDLD, 2);

            return new PayrollUnionTraceDto
            {
                IDBL = idbl,
                MANV = manv,
                MAKYCONG = makycong,
                POLICY_CD_ID = policy.ID,
                PROFILE_CD_ID = profile.ID,
                LA_DOAN_VIEN = profile.LA_DOAN_VIEN,
                LUONG_CAN_CU_DONG = insuranceBaseSalary,
                TY_LE_DOAN_PHI_NLD = policy.TY_LE_DOAN_PHI_NLD,
                MUC_TRAN_DOAN_PHI = maxCap,
                TIEN_DOAN_PHI_NLD = tienDoanPhiNld,
                TY_LE_KPCD_NSDLD = policy.KINH_PHI_CONG_DOAN_NSDLD,
                TIEN_KPCD_NSDLD = tienKpcdNsdld
            };
        }
    }
}
