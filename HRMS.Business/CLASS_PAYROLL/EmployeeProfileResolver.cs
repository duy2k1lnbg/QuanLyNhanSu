using DA;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.CLASS_PAYROLL
{
    public interface IEmployeeProfileResolver
    {
        EmployeeInsuranceProfileDto GetInsuranceProfile(decimal manv, DateTime effectiveDate);
        EmployeeUnionProfileDto GetUnionProfile(decimal manv, DateTime effectiveDate);
        EmployeeTaxProfileDto GetTaxProfile(decimal manv, DateTime effectiveDate);
        EmployeeInsuranceProfileDto GetOrProvisionInsuranceProfile(decimal manv, DateTime effectiveDate, int defaultRegion = 1);
        EmployeeUnionProfileDto GetOrProvisionUnionProfile(decimal manv, DateTime effectiveDate);
        EmployeeTaxProfileDto GetOrProvisionTaxProfile(decimal manv, DateTime effectiveDate);
        List<DependentDto> GetActiveDependents(decimal manv, int kyCong);
    }

    public class EmployeeProfileResolver : IEmployeeProfileResolver
    {
        public EmployeeInsuranceProfileDto GetInsuranceProfile(decimal manv, DateTime effectiveDate)
        {
            using (var db = new MyEntities())
            {
                return db.Database.SqlQuery<EmployeeInsuranceProfileDto>(@"
                    SELECT ID, MANV, VUNG_LUONG, THAM_GIA_BHXH, THAM_GIA_BHYT, THAM_GIA_BHTN,
                           THAM_GIA_TNLD_BNN, HUONG_TY_LE_TNLD_UU_DAI, LUONG_DONG_BHXH_RIENG,
                           NGAY_BAT_DAU, NGAY_KET_THUC, TRANG_THAI
                    FROM TB_NHANVIEN_BAOHIEM_THAM_GIA
                    WHERE MANV = :p0 AND TRANG_THAI = 'ACTIVE'
                      AND NGAY_BAT_DAU <= :p1
                      AND (NGAY_KET_THUC IS NULL OR NGAY_KET_THUC >= :p1)",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", effectiveDate)
                ).SingleOrDefault();
            }
        }

        public EmployeeUnionProfileDto GetUnionProfile(decimal manv, DateTime effectiveDate)
        {
            using (var db = new MyEntities())
            {
                return db.Database.SqlQuery<EmployeeUnionProfileDto>(@"
                    SELECT ID, MANV, LA_DOAN_VIEN, NGAY_GIA_NHAP, NGAY_KET_THUC, TRANG_THAI
                    FROM TB_NHANVIEN_CONG_DOAN_THAM_GIA
                    WHERE MANV = :p0 AND TRANG_THAI = 'ACTIVE'
                      AND NGAY_GIA_NHAP <= :p1
                      AND (NGAY_KET_THUC IS NULL OR NGAY_KET_THUC >= :p1)",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", effectiveDate)
                ).SingleOrDefault();
            }
        }

        public EmployeeTaxProfileDto GetTaxProfile(decimal manv, DateTime effectiveDate)
        {
            using (var db = new MyEntities())
            {
                return db.Database.SqlQuery<EmployeeTaxProfileDto>(@"
                    SELECT ID, MANV, MA_SO_THUE, IS_CU_TRU, CO_UY_QUYEN_QUYET_TOAN,
                           NGAY_HIEU_LUC, NGAY_HET_HIEU_LUC, TRANG_THAI
                    FROM TB_NHANVIEN_THUE
                    WHERE MANV = :p0 AND TRANG_THAI = 'ACTIVE'
                      AND NGAY_HIEU_LUC <= :p1
                      AND (NGAY_HET_HIEU_LUC IS NULL OR NGAY_HET_HIEU_LUC >= :p1)",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", effectiveDate)
                ).SingleOrDefault();
            }
        }

        public EmployeeInsuranceProfileDto GetOrProvisionInsuranceProfile(decimal manv, DateTime effectiveDate, int defaultRegion = 1)
        {
            var existing = GetInsuranceProfile(manv, effectiveDate);
            if (existing != null) return existing;

            using (var db = new MyEntities())
            {

                // Fallback / Auto-provision default insurance profile
                decimal newId = db.Database.SqlQuery<decimal>("SELECT SEQ_NV_BAOHIEM_TG.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_NHANVIEN_BAOHIEM_THAM_GIA (
                        ID, MANV, VUNG_LUONG, THAM_GIA_BHXH, THAM_GIA_BHYT, THAM_GIA_BHTN,
                        THAM_GIA_TNLD_BNN, HUONG_TY_LE_TNLD_UU_DAI, NGAY_BAT_DAU, TRANG_THAI, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, 1, 1, 1, 1, 0, DATE '2026-01-01', 'ACTIVE', SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", newId),
                    new OracleParameter("p1", manv),
                    new OracleParameter("p2", defaultRegion)
                );
                return new EmployeeInsuranceProfileDto
                {
                    ID = newId,
                    MANV = manv,
                    VUNG_LUONG = (byte)defaultRegion,
                    THAM_GIA_BHXH = 1,
                    THAM_GIA_BHYT = 1,
                    THAM_GIA_BHTN = 1,
                    THAM_GIA_TNLD_BNN = 1,
                    HUONG_TY_LE_TNLD_UU_DAI = 0,
                    NGAY_BAT_DAU = new DateTime(2026, 1, 1),
                    TRANG_THAI = "ACTIVE"
                };
            }
        }

        public EmployeeUnionProfileDto GetOrProvisionUnionProfile(decimal manv, DateTime effectiveDate)
        {
            var existing = GetUnionProfile(manv, effectiveDate);
            if (existing != null) return existing;

            using (var db = new MyEntities())
            {
                // Fallback / Auto-provision default union profile
                decimal newId = db.Database.SqlQuery<decimal>("SELECT SEQ_NV_CONGDOAN_TG.NEXTVAL FROM DUAL").First();
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_NHANVIEN_CONG_DOAN_THAM_GIA (
                        ID, MANV, LA_DOAN_VIEN, NGAY_GIA_NHAP, TRANG_THAI, CREATED_AT
                    ) VALUES (
                        :p0, :p1, 1, DATE '2026-01-01', 'ACTIVE', SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", newId),
                    new OracleParameter("p1", manv)
                );
                return new EmployeeUnionProfileDto
                {
                    ID = newId,
                    MANV = manv,
                    LA_DOAN_VIEN = 1,
                    NGAY_GIA_NHAP = new DateTime(2026, 1, 1),
                    TRANG_THAI = "ACTIVE"
                };
            }
        }

        public EmployeeTaxProfileDto GetOrProvisionTaxProfile(decimal manv, DateTime effectiveDate)
        {
            var existing = GetTaxProfile(manv, effectiveDate);
            if (existing != null) return existing;

            using (var db = new MyEntities())
            {

                // Fallback / Auto-provision default tax profile
                decimal newId = db.Database.SqlQuery<decimal>("SELECT SEQ_NV_THUE.NEXTVAL FROM DUAL").First();
                string mst = "MST" + ((long)manv).ToString().PadLeft(7, '0');
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO TB_NHANVIEN_THUE (
                        ID, MANV, MA_SO_THUE, IS_CU_TRU, CO_UY_QUYEN_QUYET_TOAN, NGAY_HIEU_LUC, TRANG_THAI, CREATED_AT
                    ) VALUES (
                        :p0, :p1, :p2, 1, 1, DATE '2026-01-01', 'ACTIVE', SYSTIMESTAMP
                    )",
                    new OracleParameter("p0", newId),
                    new OracleParameter("p1", manv),
                    new OracleParameter("p2", mst)
                );
                return new EmployeeTaxProfileDto
                {
                    ID = newId,
                    MANV = manv,
                    MA_SO_THUE = mst,
                    IS_CU_TRU = 1,
                    CO_UY_QUYEN_QUYET_TOAN = 1,
                    NGAY_HIEU_LUC = new DateTime(2026, 1, 1),
                    TRANG_THAI = "ACTIVE"
                };
            }
        }

        public List<DependentDto> GetActiveDependents(decimal manv, int kyCong)
        {
            using (var db = new MyEntities())
            {
                return db.Database.SqlQuery<DependentDto>(@"
                    SELECT ID, MANV, HO_TEN, MOI_QUAN_HE, CCCD, MA_SO_THUE_NPT, NGAY_SINH,
                           THANG_BAT_DAU_GIAM_TRU, THANG_KET_THUC_GIAM_TRU, TRANG_THAI
                    FROM TB_NGUOI_PHU_THUOC
                    WHERE MANV = :p0 AND TRANG_THAI = 'ACTIVE'
                      AND THANG_BAT_DAU_GIAM_TRU <= :p1
                      AND (THANG_KET_THUC_GIAM_TRU IS NULL OR THANG_KET_THUC_GIAM_TRU >= :p1)",
                    new OracleParameter("p0", manv),
                    new OracleParameter("p1", kyCong)
                ).ToList();
            }
        }
    }
}
