using System;
using System.Collections.Generic;

namespace Bu.CLASS_PAYROLL
{
    public class SalaryPolicyDto
    {
        public decimal ID { get; set; }
        public string MA_CHINH_SACH { get; set; }
        public string TEN_CHINH_SACH { get; set; }
        public DateTime NGAY_HIEU_LUC { get; set; }
        public DateTime? NGAY_HET_HIEU_LUC { get; set; }
        public decimal SO_CONG_CHUAN_THANG { get; set; }
        public decimal SO_GIO_CHUAN_NGAY { get; set; }
        public decimal HE_SO_LAM_DEM { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class InsurancePolicyDto
    {
        public decimal ID { get; set; }
        public string MA_CHINH_SACH { get; set; }
        public string TEN_CHINH_SACH { get; set; }
        public DateTime NGAY_HIEU_LUC { get; set; }
        public DateTime? NGAY_HET_HIEU_LUC { get; set; }
        public decimal MUC_THAM_CHIEU { get; set; }
        public decimal TY_LE_BHXH_NLD { get; set; }
        public decimal TY_LE_BHYT_NLD { get; set; }
        public decimal TY_LE_BHTN_NLD { get; set; }
        public decimal TY_LE_BHXH_NSDLD { get; set; }
        public decimal TY_LE_BHYT_NSDLD { get; set; }
        public decimal TY_LE_BHTN_NSDLD { get; set; }
        public decimal TY_LE_TNLD_BNN_NSDLD { get; set; }
        public decimal TY_LE_TNLD_BNN_UU_DAI { get; set; }
        public int AP_DUNG_TRAN_BHXH_BHYT { get; set; }
        public int AP_DUNG_TRAN_BHTN { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class InsuranceRegionDto
    {
        public decimal ID { get; set; }
        public decimal POLICY_BHXH_ID { get; set; }
        public int VUNG_LUONG { get; set; }
        public decimal LUONG_TOI_THIEU_THANG { get; set; }
        public decimal LUONG_TOI_THIEU_GIO { get; set; }
        public decimal HE_SO_SAN_DOANH_NGHIEP { get; set; }
    }

    public class UnionPolicyDto
    {
        public decimal ID { get; set; }
        public string MA_CHINH_SACH { get; set; }
        public string TEN_CHINH_SACH { get; set; }
        public DateTime NGAY_HIEU_LUC { get; set; }
        public DateTime? NGAY_HET_HIEU_LUC { get; set; }
        public decimal TY_LE_DOAN_PHI_NLD { get; set; }
        public decimal CAP_PERCENT_STATUTORY_BASE_SALARY { get; set; }
        public decimal KINH_PHI_CONG_DOAN_NSDLD { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class TaxPolicyDto
    {
        public decimal ID { get; set; }
        public string MA_CHINH_SACH { get; set; }
        public string TEN_CHINH_SACH { get; set; }
        public int TAX_YEAR { get; set; }
        public DateTime NGAY_HIEU_LUC { get; set; }
        public DateTime? NGAY_HET_HIEU_LUC { get; set; }
        public decimal GIAM_TRU_BAN_THAN_THANG { get; set; }
        public decimal GIAM_TRU_PHU_THUOC_THANG { get; set; }
        public decimal GIAM_TRU_BAN_THAN_NAM { get; set; }
        public decimal GIAM_TRU_PHU_THUOC_NAM { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class TaxBracketDto
    {
        public decimal ID { get; set; }
        public decimal POLICY_THUE_ID { get; set; }
        public int TAX_YEAR { get; set; }
        public string PERIOD_TYPE { get; set; } // 'MONTH' or 'YEAR'
        public int BAC_THUE { get; set; }
        public decimal CAN_DUOI { get; set; }
        public decimal? CAN_TREN { get; set; }
        public decimal THUE_SUAT { get; set; }
    }

    public class EmployeeInsuranceProfileDto
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public int VUNG_LUONG { get; set; }
        public int THAM_GIA_BHXH { get; set; }
        public int THAM_GIA_BHYT { get; set; }
        public int THAM_GIA_BHTN { get; set; }
        public int THAM_GIA_TNLD_BNN { get; set; }
        public int HUONG_TY_LE_TNLD_UU_DAI { get; set; }
        public decimal? LUONG_DONG_BHXH_RIENG { get; set; }
        public DateTime NGAY_BAT_DAU { get; set; }
        public DateTime? NGAY_KET_THUC { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class EmployeeUnionProfileDto
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public int LA_DOAN_VIEN { get; set; }
        public DateTime NGAY_GIA_NHAP { get; set; }
        public DateTime? NGAY_KET_THUC { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class EmployeeTaxProfileDto
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public string MA_SO_THUE { get; set; }
        public int IS_CU_TRU { get; set; }
        public int CO_UY_QUYEN_QUYET_TOAN { get; set; }
        public DateTime NGAY_HIEU_LUC { get; set; }
        public DateTime? NGAY_HET_HIEU_LUC { get; set; }
        public string TRANG_THAI { get; set; }
    }

    public class DependentDto
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public string HO_TEN { get; set; }
        public string MOI_QUAN_HE { get; set; }
        public string CCCD { get; set; }
        public string MA_SO_THUE_NPT { get; set; }
        public DateTime? NGAY_SINH { get; set; }
        public int THANG_BAT_DAU_GIAM_TRU { get; set; } // YYYYMM
        public int? THANG_KET_THUC_GIAM_TRU { get; set; } // YYYYMM
        public string TRANG_THAI { get; set; }
    }

    public class OtComplianceSnapshotDto
    {
        public decimal ID { get; set; }
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public decimal MONTHLY_STANDARD_LIMIT { get; set; }
        public decimal ANNUAL_STANDARD_LIMIT { get; set; }
        public decimal ANNUAL_EXTENDED_LIMIT { get; set; }
        public decimal ACTUAL_HOURS_MONTH { get; set; }
        public decimal ACTUAL_HOURS_YTD { get; set; }
        public decimal LEGAL_HOURS_MONTH { get; set; }
        public decimal EXCESS_HOURS_MONTH { get; set; }
        public int? EXTENDED_ELIGIBLE { get; set; }
        public int? EMPLOYEE_CONSENT { get; set; }
        public int? NOTIFICATION_FILED { get; set; }
        public string COMPLIANCE_STATUS { get; set; }
        public decimal? TOTAL_OT_PAYMENT { get; set; }
        public decimal? LEGAL_ALLOWED_PAYMENT { get; set; }
        public decimal? EXEMPT_OT_PAYMENT { get; set; }
        public decimal? TAXABLE_EXCESS_PAYMENT { get; set; }
    }

    public class PayrollDetailItemDto
    {
        public decimal IDBLCT { get; set; }
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public string NHOM_KHOAN_MUC { get; set; }
        public string MA_KHOAN_MUC { get; set; }
        public string TEN_KHOAN_MUC { get; set; }
        public decimal SO_LUONG { get; set; }
        public decimal DON_GIA { get; set; }
        public decimal HE_SO { get; set; }
        public decimal THANH_TIEN { get; set; }
        public int TINH_VAO_DONG_BHXH { get; set; }
        public int TINH_THUE_TNCN { get; set; }
        public decimal? SO_TIEN_MIEN_THUE { get; set; }
        public decimal? SO_TIEN_CHIU_THUE { get; set; }
        public string CONG_THUC_DIEN_GIAI { get; set; }
        public int IS_LEGACY { get; set; }
        public List<PayrollDetailSourceDto> Sources { get; set; } = new List<PayrollDetailSourceDto>();
    }

    public class PayrollDetailSourceDto
    {
        public decimal ID { get; set; }
        public decimal IDBLCT { get; set; }
        public decimal MANV { get; set; }
        public string SOURCE_TYPE { get; set; }
        public decimal? SOURCE_BCCT_ID { get; set; }
        public decimal? SOURCE_TC_ID { get; set; }
        public decimal? SOURCE_NVPC_ID { get; set; }
        public string SOURCE_KTKL_SOQD { get; set; }
        public decimal? SOURCE_UL_ID { get; set; }
        public decimal WEIGHT_QUANTITY { get; set; }
        public decimal AMOUNT_CONTRIBUTED { get; set; }
    }

    public class PayrollInsuranceTraceDto
    {
        public decimal ID { get; set; }
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public decimal POLICY_BHXH_ID { get; set; }
        public decimal PROFILE_BH_ID { get; set; }
        public decimal MUC_THAM_CHIEU { get; set; }
        public int VUNG_LUONG { get; set; }
        public decimal LUONG_TOI_THIEU_VUNG { get; set; }
        public decimal LUONG_DONG_BHXH_GOC { get; set; }
        public decimal LUONG_DONG_BHXH_AP_DUNG { get; set; }
        public decimal LUONG_DONG_BHTN_AP_DUNG { get; set; }
        public decimal TY_LE_BHXH_NLD { get; set; }
        public decimal TIEN_BHXH_NLD { get; set; }
        public decimal TY_LE_BHYT_NLD { get; set; }
        public decimal TIEN_BHYT_NLD { get; set; }
        public decimal TY_LE_BHTN_NLD { get; set; }
        public decimal TIEN_BHTN_NLD { get; set; }
        public decimal TONG_BH_NLD { get; set; }
        public decimal TY_LE_BHXH_NSDLD { get; set; }
        public decimal TIEN_BHXH_NSDLD { get; set; }
        public decimal TY_LE_BHYT_NSDLD { get; set; }
        public decimal TIEN_BHYT_NSDLD { get; set; }
        public decimal TY_LE_BHTN_NSDLD { get; set; }
        public decimal TIEN_BHTN_NSDLD { get; set; }
        public decimal TY_LE_TNLD_BNN_NSDLD { get; set; }
        public decimal TIEN_TNLD_BNN_NSDLD { get; set; }
        public decimal TONG_BH_NSDLD { get; set; }
    }

    public class PayrollUnionTraceDto
    {
        public decimal ID { get; set; }
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public decimal POLICY_CD_ID { get; set; }
        public decimal PROFILE_CD_ID { get; set; }
        public int LA_DOAN_VIEN { get; set; }
        public decimal LUONG_CAN_CU_DONG { get; set; }
        public decimal TY_LE_DOAN_PHI_NLD { get; set; }
        public decimal MUC_TRAN_DOAN_PHI { get; set; }
        public decimal TIEN_DOAN_PHI_NLD { get; set; }
        public decimal TY_LE_KPCD_NSDLD { get; set; }
        public decimal TIEN_KPCD_NSDLD { get; set; }
    }

    public class PayrollTaxTraceDto
    {
        public decimal ID { get; set; }
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public decimal POLICY_THUE_ID { get; set; }
        public int BAC_THUE { get; set; }
        public decimal CAN_DUOI { get; set; }
        public decimal? CAN_TREN { get; set; }
        public decimal THU_NHAP_CHIU_THUE_BAC { get; set; }
        public decimal THUE_SUAT { get; set; }
        public decimal TIEN_THUE_BAC { get; set; }
    }

    public class AnnualTaxFinalizationDto
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public int TAX_YEAR { get; set; }
        public decimal POLICY_THUE_ID { get; set; }
        public decimal PROFILE_THUE_ID { get; set; }
        public int SO_THANG_LAM_VIEC { get; set; }
        public decimal TONG_THU_NHAP_CHIU_THUE { get; set; }
        public decimal GIAM_TRU_BAN_THAN { get; set; }
        public decimal GIAM_TRU_PHU_THUOC { get; set; }
        public decimal GIAM_TRU_BAO_HIEM { get; set; }
        public decimal GIAM_TRU_KHAC { get; set; }
        public decimal TONG_GIAM_TRU { get; set; }
        public decimal THU_NHAP_TINH_THUE { get; set; }
        public decimal TONG_THUE_PHAI_NOP { get; set; }
        public decimal THUE_DA_KHAU_TRU { get; set; }
        public decimal THUE_CON_PHAI_NOP { get; set; }
        public decimal THUE_NOP_THUA { get; set; }
        public string TRANG_THAI { get; set; }
        public List<AnnualTaxFinalizationBracketDto> Brackets { get; set; } = new List<AnnualTaxFinalizationBracketDto>();
    }

    public class AnnualTaxFinalizationBracketDto
    {
        public decimal ID { get; set; }
        public decimal QUYET_TOAN_ID { get; set; }
        public decimal MANV { get; set; }
        public int TAX_YEAR { get; set; }
        public int BAC_THUE { get; set; }
        public decimal CAN_DUOI { get; set; }
        public decimal? CAN_TREN { get; set; }
        public decimal THU_NHAP_CHIU_THUE_BAC { get; set; }
        public decimal THUE_SUAT { get; set; }
        public decimal TIEN_THUE_BAC { get; set; }
    }

    public class PayrollRunDto
    {
        public decimal RUN_ID { get; set; }
        public decimal MAKYCONG { get; set; }
        public int NAM { get; set; }
        public int THANG { get; set; }
        public string EXECUTION_TYPE { get; set; }
        public int TOTAL_EMPLOYEES { get; set; }
        public int SUCCESS_COUNT { get; set; }
        public int WARNING_COUNT { get; set; }
        public int ERROR_COUNT { get; set; }
        public string STATUS { get; set; }
        public string ERROR_SUMMARY { get; set; }
        public DateTime STARTED_AT { get; set; }
        public DateTime? FINISHED_AT { get; set; }
        public string EXECUTED_BY { get; set; }
    }
}
