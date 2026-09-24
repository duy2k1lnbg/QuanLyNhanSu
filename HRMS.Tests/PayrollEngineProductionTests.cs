using Bu.CLASS_PAYROLL;
using DA;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class PayrollEngineProductionTests
    {
        private const string ConnectionString = "DATA SOURCE=localhost:1521/orcl;PASSWORD=hr;USER ID=HR";
        private IPolicyResolver _policyResolver;
        private IInsuranceEngine _insuranceEngine;
        private IUnionEngine _unionEngine;
        private ITaxEngine _taxEngine;
        private IOtComplianceEngine _otEngine;
        private IAnnualTaxFinalizationService _finalizationService;
        private IPayrollEngine _payrollEngine;

        [SetUp]
        public void Setup()
        {
            _policyResolver = new PolicyResolver();
            _insuranceEngine = new InsuranceEngine();
            _unionEngine = new UnionEngine();
            _taxEngine = new TaxEngine();
            _otEngine = new OtComplianceEngine();
            _finalizationService = new AnnualTaxFinalizationService();
            _payrollEngine = new PayrollEngine(_policyResolver, new EmployeeProfileResolver(), _otEngine, _insuranceEngine, _unionEngine, _taxEngine);
        }

        [Test]
        public void Test_01_PolicyResolver_H1_And_H2_EffectiveDatedResolution()
        {
            // H1 2026: 01/01/2026 -> 30/06/2026 (Mức tham chiếu 2,340,000)
            var h1Date = new DateTime(2026, 3, 31);
            var h1Policy = _policyResolver.GetInsurancePolicy(h1Date);
            Assert.AreEqual("BHXH_2026_H1", h1Policy.MA_CHINH_SACH);
            Assert.AreEqual(2340000m, h1Policy.MUC_THAM_CHIEU);

            var h1Regions = _policyResolver.GetInsuranceRegions(h1Policy.ID);
            Assert.AreEqual(4, h1Regions.Count);
            Assert.AreEqual(5310000m, h1Regions[0].LUONG_TOI_THIEU_THANG); // Vùng I
            Assert.AreEqual(4730000m, h1Regions[1].LUONG_TOI_THIEU_THANG); // Vùng II
            Assert.AreEqual(4140000m, h1Regions[2].LUONG_TOI_THIEU_THANG); // Vùng III
            Assert.AreEqual(3700000m, h1Regions[3].LUONG_TOI_THIEU_THANG); // Vùng IV

            // H2 2026: 01/07/2026 -> NULL (Mức tham chiếu 2,530,000 theo NĐ 161/2026)
            var h2Date = new DateTime(2026, 7, 31);
            var h2Policy = _policyResolver.GetInsurancePolicy(h2Date);
            Assert.AreEqual("BHXH_2026_H2", h2Policy.MA_CHINH_SACH);
            Assert.AreEqual(2530000m, h2Policy.MUC_THAM_CHIEU);

            // Fail-fast test: Date outside any policy
            Assert.Throws<PolicyNotFoundException>(() =>
            {
                _policyResolver.GetInsurancePolicy(new DateTime(2020, 1, 1));
            });
        }

        [Test]
        public void Test_02_TaxPolicy_Contiguous5Brackets_MonthlyAndYearly()
        {
            var taxPolicy = _policyResolver.GetTaxPolicy(2026, new DateTime(2026, 5, 1));
            Assert.AreEqual(15500000m, taxPolicy.GIAM_TRU_BAN_THAN_THANG);
            Assert.AreEqual(6200000m, taxPolicy.GIAM_TRU_PHU_THUOC_THANG);
            Assert.AreEqual(186000000m, taxPolicy.GIAM_TRU_BAN_THAN_NAM);
            Assert.AreEqual(74400000m, taxPolicy.GIAM_TRU_PHU_THUOC_NAM);

            // Month brackets
            var monthBrackets = _policyResolver.GetTaxBrackets(taxPolicy.ID, "MONTH");
            Assert.AreEqual(5, monthBrackets.Count);
            Assert.AreEqual(0m, monthBrackets[0].CAN_DUOI);
            Assert.AreEqual(10000000m, monthBrackets[0].CAN_TREN);
            Assert.AreEqual(0.05m, monthBrackets[0].THUE_SUAT);

            Assert.AreEqual(10000000m, monthBrackets[1].CAN_DUOI);
            Assert.AreEqual(30000000m, monthBrackets[1].CAN_TREN);
            Assert.AreEqual(0.10m, monthBrackets[1].THUE_SUAT);

            Assert.AreEqual(30000000m, monthBrackets[2].CAN_DUOI);
            Assert.AreEqual(60000000m, monthBrackets[2].CAN_TREN);
            Assert.AreEqual(0.20m, monthBrackets[2].THUE_SUAT);

            Assert.AreEqual(60000000m, monthBrackets[3].CAN_DUOI);
            Assert.AreEqual(100000000m, monthBrackets[3].CAN_TREN);
            Assert.AreEqual(0.30m, monthBrackets[3].THUE_SUAT);

            Assert.AreEqual(100000000m, monthBrackets[4].CAN_DUOI);
            Assert.IsNull(monthBrackets[4].CAN_TREN);
            Assert.AreEqual(0.35m, monthBrackets[4].THUE_SUAT);

            // Year brackets
            var yearBrackets = _policyResolver.GetTaxBrackets(taxPolicy.ID, "YEAR");
            Assert.AreEqual(5, yearBrackets.Count);
            Assert.AreEqual(0m, yearBrackets[0].CAN_DUOI);
            Assert.AreEqual(120000000m, yearBrackets[0].CAN_TREN);
            Assert.AreEqual(1200000000m, yearBrackets[4].CAN_DUOI);
            Assert.IsNull(yearBrackets[4].CAN_TREN);
        }

        [Test]
        public void Test_03_InsuranceEngine_Floor_And_Cap_Calculations()
        {
            var h1Policy = _policyResolver.GetInsurancePolicy(new DateTime(2026, 2, 1));
            var region1 = _policyResolver.GetInsuranceRegion(h1Policy.ID, 1); // 5,310,000

            var profileStandard = new EmployeeInsuranceProfileDto
            {
                ID = 1,
                MANV = 9999,
                VUNG_LUONG = 1,
                THAM_GIA_BHXH = 1,
                THAM_GIA_BHYT = 1,
                THAM_GIA_BHTN = 1,
                THAM_GIA_TNLD_BNN = 1,
                HUONG_TY_LE_TNLD_UU_DAI = 0
            };

            // Case A: Salary below regional floor (e.g., 4,000,000) -> clamped to 5,310,000
            var resA = _insuranceEngine.CalculateInsurance(1, 9999, 202602, 4000000m, h1Policy, region1, profileStandard);
            Assert.AreEqual(5310000m, resA.LUONG_DONG_BHXH_AP_DUNG);
            Assert.AreEqual(Math.Round(5310000m * 0.08m, 2), resA.TIEN_BHXH_NLD);
            Assert.AreEqual(Math.Round(5310000m * 0.015m, 2), resA.TIEN_BHYT_NLD);
            Assert.AreEqual(Math.Round(5310000m * 0.01m, 2), resA.TIEN_BHTN_NLD);

            // Case B: Salary above 20x reference salary (e.g., 60,000,000 in H1) -> clamped to 46,800,000
            var resB = _insuranceEngine.CalculateInsurance(1, 9999, 202602, 60000000m, h1Policy, region1, profileStandard);
            Assert.AreEqual(46800000m, resB.LUONG_DONG_BHXH_AP_DUNG);
            Assert.AreEqual(Math.Round(46800000m * 0.08m, 2), resB.TIEN_BHXH_NLD); // 3,744,000

            // Case C: Preferential TNLĐ-BNN rate (0.3% under Decree 58/2020)
            var profileUuDai = new EmployeeInsuranceProfileDto
            {
                ID = 2,
                MANV = 9998,
                VUNG_LUONG = 1,
                THAM_GIA_BHXH = 1,
                THAM_GIA_BHYT = 1,
                THAM_GIA_BHTN = 1,
                THAM_GIA_TNLD_BNN = 1,
                HUONG_TY_LE_TNLD_UU_DAI = 1
            };
            var resC = _insuranceEngine.CalculateInsurance(1, 9998, 202602, 10000000m, h1Policy, region1, profileUuDai);
            Assert.AreEqual(0.003m, resC.TY_LE_TNLD_BNN_NSDLD);
            Assert.AreEqual(30000m, resC.TIEN_TNLD_BNN_NSDLD);
        }

        [Test]
        public void Test_04_UnionEngine_Decision61_Cap_Calculation()
        {
            var unionPolicy = _policyResolver.GetUnionPolicy(new DateTime(2026, 2, 1));
            var profileUnion = new EmployeeUnionProfileDto { ID = 1, MANV = 9999, LA_DOAN_VIEN = 1 };
            var profileNonUnion = new EmployeeUnionProfileDto { ID = 2, MANV = 9998, LA_DOAN_VIEN = 0 };

            // In H1 2026: Base salary 2,340,000 -> 10% cap = 234,000đ
            // High salary (60,000,000): 0.5% = 300,000đ -> capped at 234,000đ
            var resH1High = _unionEngine.CalculateUnion(1, 9999, 202602, 60000000m, unionPolicy, profileUnion, 2340000m);
            Assert.AreEqual(234000m, resH1High.MUC_TRAN_DOAN_PHI);
            Assert.AreEqual(234000m, resH1High.TIEN_DOAN_PHI_NLD);
            Assert.AreEqual(1200000m, resH1High.TIEN_KPCD_NSDLD); // 2% employer

            // In H2 2026: Base salary 2,530,000 -> 10% cap = 253,000đ
            var resH2High = _unionEngine.CalculateUnion(1, 9999, 202608, 60000000m, unionPolicy, profileUnion, 2530000m);
            Assert.AreEqual(253000m, resH2High.MUC_TRAN_DOAN_PHI);
            Assert.AreEqual(253000m, resH2High.TIEN_DOAN_PHI_NLD);

            // Non-union employee pays 0đ
            var resNonUnion = _unionEngine.CalculateUnion(1, 9998, 202602, 10000000m, unionPolicy, profileNonUnion, 2340000m);
            Assert.AreEqual(0m, resNonUnion.TIEN_DOAN_PHI_NLD);
            Assert.AreEqual(200000m, resNonUnion.TIEN_KPCD_NSDLD); // employer still pays 2%
        }

        [Test]
        public void Test_05_TaxEngine_Monthly5Brackets_And_Deductions()
        {
            var taxPolicy = _policyResolver.GetTaxPolicy(2026, new DateTime(2026, 2, 1));
            var brackets = _policyResolver.GetTaxBrackets(taxPolicy.ID, "MONTH");

            // Case A: Gross 20,000,000, 1 dependent (6.2M), Insurance 2,100,000
            // Deductions = 15.5M + 6.2M + 2.1M = 23.8M > Gross -> Tax = 0
            var resA = _taxEngine.CalculateMonthlyTax(1, 9999, 202602, 20000000m, 1, 2100000m, taxPolicy, brackets);
            Assert.AreEqual(0m, resA.TotalTax);
            Assert.AreEqual(0m, resA.TaxableAssessableIncome);

            // Case B: Gross 50,000,000, 0 dependents, Insurance 4,200,000
            // Deductions = 15.5M + 4.2M = 19.7M
            // Taxable income = 50M - 19.7M = 30,300,000đ
            // Bracket 1 (0 -> 10M, 5%): 10M * 0.05 = 500,000đ
            // Bracket 2 (10M -> 30M, 10%): 20M * 0.10 = 2,000,000đ
            // Bracket 3 (30M -> 60M, 20%): 300,000 * 0.20 = 60,000đ
            // Total Tax = 500k + 2,000k + 60k = 2,560,000đ
            var resB = _taxEngine.CalculateMonthlyTax(1, 9999, 202602, 50000000m, 0, 4200000m, taxPolicy, brackets);
            Assert.AreEqual(30300000m, resB.TaxableAssessableIncome);
            Assert.AreEqual(2560000m, resB.TotalTax);
            Assert.AreEqual(3, resB.BracketTraces.Count);
        }

        [Test]
        public void Test_06_OtComplianceEngine_Decree253_TaxSplit()
        {
            // Case A: 30 hours OT (within 40h standard monthly limit, within 200h YTD)
            var resA = _otEngine.EvaluateCompliance(
                1, 9999, 202602, 30m, 50m, 6000000m, 100000m
            );
            Assert.AreEqual("COMPLIANT", resA.COMPLIANCE_STATUS);
            Assert.AreEqual(30m, resA.LEGAL_HOURS_MONTH);
            Assert.AreEqual(0m, resA.EXCESS_HOURS_MONTH);
            Assert.AreEqual(6000000m, resA.EXEMPT_OT_PAYMENT);
            Assert.AreEqual(0m, resA.TAXABLE_EXCESS_PAYMENT);

            // Case B: 50 hours OT (exceeds 40h monthly cap)
            // 40h legal (exempt), 10h excess (taxable)
            var resB = _otEngine.EvaluateCompliance(
                1, 9999, 202602, 50m, 50m, 10000000m, 100000m
            );
            Assert.AreEqual("EXCEEDED_LEGAL_CAP", resB.COMPLIANCE_STATUS);
            Assert.AreEqual(40m, resB.LEGAL_HOURS_MONTH);
            Assert.AreEqual(10m, resB.EXCESS_HOURS_MONTH);
            Assert.AreEqual(8000000m, resB.EXEMPT_OT_PAYMENT); // 40/50 * 10M
            Assert.AreEqual(2000000m, resB.TAXABLE_EXCESS_PAYMENT); // 10/50 * 10M
            Assert.AreEqual(resB.TOTAL_OT_PAYMENT, resB.EXEMPT_OT_PAYMENT + resB.TAXABLE_EXCESS_PAYMENT);
        }

        [Test]
        public void Test_07_ReferencedPolicyImmutability_TriggerEnforcement()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                // Check if any policy is referenced in TB_BANGLUONG
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM TB_BANGLUONG 
                    WHERE POLICY_LUONG_ID IS NOT NULL AND ROWNUM = 1", conn))
                {
                    int refCount = Convert.ToInt32(cmd.ExecuteScalar());
                    if (refCount == 0)
                    {
                        // Temporarily set policy reference on a dummy record to verify trigger
                        using (var dummyCmd = new OracleCommand(@"
                            UPDATE TB_BANGLUONG 
                            SET POLICY_LUONG_ID = (SELECT ID FROM TB_CHINH_SACH_LUONG WHERE ROWNUM = 1)
                            WHERE IDBL = 1934", conn))
                        {
                            dummyCmd.ExecuteNonQuery();
                        }
                    }

                    // Attempting to UPDATE the referenced salary policy MUST fail with ORA-20021
                    var ex = Assert.Throws<OracleException>(() =>
                    {
                        using (var updateCmd = new OracleCommand(@"
                            UPDATE TB_CHINH_SACH_LUONG 
                            SET SO_CONG_CHUAN_THANG = 25 
                            WHERE ID = (SELECT POLICY_LUONG_ID FROM TB_BANGLUONG WHERE POLICY_LUONG_ID IS NOT NULL AND ROWNUM = 1)", conn))
                        {
                            updateCmd.ExecuteNonQuery();
                        }
                    });

                    Console.WriteLine($"Policy immutability trigger successfully blocked update with: {ex.Message}");
                    StringAssert.Contains("ORA-20021", ex.Message);
                    StringAssert.Contains("REFERENCED POLICY IMMUTABILITY", ex.Message);

                    // Revert dummy reference on IDBL = 1934 to maintain clean legacy state
                    using (var revertCmd = new OracleCommand("UPDATE TB_BANGLUONG SET POLICY_LUONG_ID = NULL WHERE IDBL = 1934", conn))
                    {
                        revertCmd.ExecuteNonQuery();
                    }
                }
            }
        }

        [Test]
        public void Test_08_AnnualTaxFinalization_CalculationAndSettlement()
        {
            var taxPolicy = _policyResolver.GetTaxPolicy(2026, new DateTime(2026, 12, 31));
            var yearBrackets = _policyResolver.GetTaxBrackets(taxPolicy.ID, "YEAR");
            var profile = new EmployeeTaxProfileDto { ID = 1, MANV = 2453, IS_CU_TRU = 1 };

            var finalization = _finalizationService.CalculateAnnualTaxFinalization(
                2453, 2026, taxPolicy, yearBrackets, profile
            );

            Assert.IsNotNull(finalization);
            Assert.AreEqual(2453, finalization.MANV);
            Assert.AreEqual(2026, finalization.TAX_YEAR);
            Assert.AreEqual(186000000m, finalization.GIAM_TRU_BAN_THAN);
            Assert.AreEqual(finalization.TONG_THUE_PHAI_NOP, finalization.THUE_DA_KHAU_TRU + finalization.THUE_CON_PHAI_NOP - finalization.THUE_NOP_THUA);
        }

        [Test]
        public void Test_09_ExecuteFullPayrollRecalculation_IntegrationTest()
        {
            // Clean any existing modern run in 202609
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG_CT_SOURCE WHERE IDBLCT IN (SELECT IDBLCT FROM TB_BANGLUONG_CT WHERE MAKYCONG = 202609)", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG_CT WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG_BAOHIEM WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG_CONG_DOAN WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG_THUE_CT WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG_OT_COMPLIANCE WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_BANGLUONG WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
                using (var cmd = new OracleCommand(@"
                    DELETE FROM TB_PAYROLL_CALCULATION_RUN WHERE MAKYCONG = 202609", conn))
                    cmd.ExecuteNonQuery();
            }

            // Execute recalculation for period 202609 (Sep 2026 - H2 rules)
            var run = _payrollEngine.ExecuteFullPayrollRecalculation(2026, 9, "UNIT_TEST_USER");
            Assert.IsNotNull(run);
            Assert.Greater(run.RUN_ID, 0);
            Assert.AreEqual("SUCCESS", run.STATUS);
            Assert.Greater(run.SUCCESS_COUNT, 0);

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                // 1. Verify modern calculated rows in 202609
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM TB_BANGLUONG
                    WHERE MAKYCONG = 202609 
                      AND IS_LEGACY = 0 
                      AND TRANG_THAI = 'CALCULATED'
                      AND POLICY_LUONG_ID IS NOT NULL
                      AND POLICY_BHXH_ID IS NOT NULL
                      AND POLICY_CONGDOAN_ID IS NOT NULL
                      AND POLICY_THUE_ID IS NOT NULL
                      AND PROFILE_BH_ID IS NOT NULL
                      AND PROFILE_CD_ID IS NOT NULL
                      AND PROFILE_THUE_ID IS NOT NULL", conn))
                {
                    int modernCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.Greater(modernCount, 0, "Expected at least 1 calculated modern payroll row in 202609");
                    Console.WriteLine($"Total modern payroll rows calculated in 202609: {modernCount}");
                }

                // 2. Verify child detail items in TB_BANGLUONG_CT
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM TB_BANGLUONG_CT
                    WHERE MAKYCONG = 202609", conn))
                {
                    int ctCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.Greater(ctCount, 0, "Expected itemized detail items in TB_BANGLUONG_CT");
                    Console.WriteLine($"Total itemized details generated in 202609: {ctCount}");
                }

                // 3. Verify insurance traces in TB_BANGLUONG_BAOHIEM
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM TB_BANGLUONG_BAOHIEM
                    WHERE MAKYCONG = 202609", conn))
                {
                    int bhCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.Greater(bhCount, 0, "Expected insurance traces in TB_BANGLUONG_BAOHIEM");
                    Console.WriteLine($"Total insurance traces generated in 202609: {bhCount}");
                }

                // 4. Verify union traces in TB_BANGLUONG_CONG_DOAN
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM TB_BANGLUONG_CONG_DOAN
                    WHERE MAKYCONG = 202609", conn))
                {
                    int cdCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.Greater(cdCount, 0, "Expected union traces in TB_BANGLUONG_CONG_DOAN");
                    Console.WriteLine($"Total union traces generated in 202609: {cdCount}");
                }

                // 5. Verify OT compliance snapshots in TB_BANGLUONG_OT_COMPLIANCE
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM TB_BANGLUONG_OT_COMPLIANCE
                    WHERE MAKYCONG = 202609", conn))
                {
                    int otCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.Greater(otCount, 0, "Expected OT compliance snapshots in TB_BANGLUONG_OT_COMPLIANCE");
                    Console.WriteLine($"Total OT compliance snapshots generated in 202609: {otCount}");
                }

                // 6. Assert that IDBL = 1934 in 202601 remains strictly legacy and unchanged!
                var fp = Idbl1934Fingerprint.Capture(conn);
                Assert.AreEqual("928e820148a1212e07a37efd3350578b27f55a3c8fde7fc5c61ba8f48dcad04c", fp.ComputeNormalizedHash(), "IDBL=1934 legacy row MUST remain 100% immutable!");

                using (var cmd = new OracleCommand("SELECT IS_LEGACY, TRANG_THAI FROM TB_BANGLUONG WHERE IDBL = 1934", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read());
                    Assert.AreEqual(1, Convert.ToInt32(reader["IS_LEGACY"]));
                    Assert.AreEqual("LEGACY_READONLY", reader["TRANG_THAI"].ToString());
                }
                Console.WriteLine("IDBL=1934 legacy immutability successfully asserted after full payroll recalculation!");
            }
        }
    }
}
