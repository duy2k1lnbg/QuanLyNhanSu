using DA;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Bu.Tests
{
    public class Idbl1934Fingerprint
    {
        public decimal IDBL { get; set; }
        public decimal MANV { get; set; }
        public decimal MAKYCONG { get; set; }
        public byte THANG { get; set; }
        public short NAM { get; set; }
        public decimal? CONG_CHUAN { get; set; }
        public decimal? CONG_THUCTE { get; set; }
        public decimal? CONG_LAMDEM { get; set; }
        public decimal? DAILY_RATE { get; set; }
        public decimal? DAILY_ALLOWANCE { get; set; }
        public decimal? LUONG_CONG_THUCTE { get; set; }
        public decimal? PHUCAP_CONG_THUCTE { get; set; }
        public decimal? TIEN_TANGCA { get; set; }
        public decimal? TIEN_CHUYENCAN { get; set; }
        public decimal? TIEN_AN_CA { get; set; }
        public decimal? KHOAN_CONG_KHAC { get; set; }
        public decimal? TIEN_BHXH_TRICH { get; set; }
        public decimal? TIEN_TAMUNG { get; set; }
        public decimal? KHOAN_TRU_KHAC { get; set; }
        public decimal? THUC_LINH { get; set; }

        public string ComputeNormalizedHash()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("IDBL={0}|", IDBL);
            sb.AppendFormat("MANV={0}|", MANV);
            sb.AppendFormat("MAKYCONG={0}|", MAKYCONG);
            sb.AppendFormat("THANG={0}|", THANG);
            sb.AppendFormat("NAM={0}|", NAM);
            sb.AppendFormat("CONG_CHUAN={0:F2}|", CONG_CHUAN);
            sb.AppendFormat("CONG_THUCTE={0:F2}|", CONG_THUCTE);
            sb.AppendFormat("CONG_LAMDEM={0:F2}|", CONG_LAMDEM);
            sb.AppendFormat("DAILY_RATE={0:F2}|", DAILY_RATE);
            sb.AppendFormat("DAILY_ALLOWANCE={0:F2}|", DAILY_ALLOWANCE);
            sb.AppendFormat("LUONG_CONG_THUCTE={0:F2}|", LUONG_CONG_THUCTE);
            sb.AppendFormat("PHUCAP_CONG_THUCTE={0:F2}|", PHUCAP_CONG_THUCTE);
            sb.AppendFormat("TIEN_TANGCA={0:F2}|", TIEN_TANGCA);
            sb.AppendFormat("TIEN_CHUYENCAN={0:F2}|", TIEN_CHUYENCAN);
            sb.AppendFormat("TIEN_AN_CA={0:F2}|", TIEN_AN_CA);
            sb.AppendFormat("KHOAN_CONG_KHAC={0:F2}|", KHOAN_CONG_KHAC);
            sb.AppendFormat("TIEN_BHXH_TRICH={0:F2}|", TIEN_BHXH_TRICH);
            sb.AppendFormat("TIEN_TAMUNG={0:F2}|", TIEN_TAMUNG);
            sb.AppendFormat("KHOAN_TRU_KHAC={0:F2}|", KHOAN_TRU_KHAC);
            sb.AppendFormat("THUC_LINH={0:F2}", THUC_LINH);

            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static Idbl1934Fingerprint Capture(OracleConnection conn)
        {
            const string sql = @"
                SELECT IDBL, MANV, MAKYCONG, THANG, NAM,
                       CONG_CHUAN, CONG_THUCTE, CONG_LAMDEM,
                       DAILY_RATE, DAILY_ALLOWANCE, LUONG_CONG_THUCTE, PHUCAP_CONG_THUCTE,
                       TIEN_TANGCA, TIEN_CHUYENCAN, TIEN_AN_CA, KHOAN_CONG_KHAC,
                       TIEN_BHXH_TRICH, TIEN_TAMUNG, KHOAN_TRU_KHAC, THUC_LINH
                FROM TB_BANGLUONG
                WHERE IDBL = 1934";

            using (var cmd = new OracleCommand(sql, conn))
            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.Read())
                    throw new InvalidOperationException("IDBL = 1934 not found in TB_BANGLUONG");

                return new Idbl1934Fingerprint
                {
                    IDBL = Convert.ToDecimal(reader["IDBL"]),
                    MANV = Convert.ToDecimal(reader["MANV"]),
                    MAKYCONG = Convert.ToDecimal(reader["MAKYCONG"]),
                    THANG = Convert.ToByte(reader["THANG"]),
                    NAM = Convert.ToInt16(reader["NAM"]),
                    CONG_CHUAN = reader["CONG_CHUAN"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["CONG_CHUAN"]),
                    CONG_THUCTE = reader["CONG_THUCTE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["CONG_THUCTE"]),
                    CONG_LAMDEM = reader["CONG_LAMDEM"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["CONG_LAMDEM"]),
                    DAILY_RATE = reader["DAILY_RATE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["DAILY_RATE"]),
                    DAILY_ALLOWANCE = reader["DAILY_ALLOWANCE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["DAILY_ALLOWANCE"]),
                    LUONG_CONG_THUCTE = reader["LUONG_CONG_THUCTE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["LUONG_CONG_THUCTE"]),
                    PHUCAP_CONG_THUCTE = reader["PHUCAP_CONG_THUCTE"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["PHUCAP_CONG_THUCTE"]),
                    TIEN_TANGCA = reader["TIEN_TANGCA"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TIEN_TANGCA"]),
                    TIEN_CHUYENCAN = reader["TIEN_CHUYENCAN"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TIEN_CHUYENCAN"]),
                    TIEN_AN_CA = reader["TIEN_AN_CA"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TIEN_AN_CA"]),
                    KHOAN_CONG_KHAC = reader["KHOAN_CONG_KHAC"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["KHOAN_CONG_KHAC"]),
                    TIEN_BHXH_TRICH = reader["TIEN_BHXH_TRICH"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TIEN_BHXH_TRICH"]),
                    TIEN_TAMUNG = reader["TIEN_TAMUNG"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["TIEN_TAMUNG"]),
                    KHOAN_TRU_KHAC = reader["KHOAN_TRU_KHAC"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["KHOAN_TRU_KHAC"]),
                    THUC_LINH = reader["THUC_LINH"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["THUC_LINH"])
                };
            }
        }

        public void AssertExactMatch(Idbl1934Fingerprint other)
        {
            Assert.AreEqual(IDBL, other.IDBL, "IDBL mismatch");
            Assert.AreEqual(MANV, other.MANV, "MANV mismatch");
            Assert.AreEqual(MAKYCONG, other.MAKYCONG, "MAKYCONG mismatch");
            Assert.AreEqual(THANG, other.THANG, "THANG mismatch");
            Assert.AreEqual(NAM, other.NAM, "NAM mismatch");
            Assert.AreEqual(CONG_CHUAN, other.CONG_CHUAN, "CONG_CHUAN mismatch");
            Assert.AreEqual(CONG_THUCTE, other.CONG_THUCTE, "CONG_THUCTE mismatch");
            Assert.AreEqual(CONG_LAMDEM, other.CONG_LAMDEM, "CONG_LAMDEM mismatch");
            Assert.AreEqual(Math.Round(DAILY_RATE ?? 0, 2), Math.Round(other.DAILY_RATE ?? 0, 2), "DAILY_RATE mismatch");
            Assert.AreEqual(Math.Round(DAILY_ALLOWANCE ?? 0, 2), Math.Round(other.DAILY_ALLOWANCE ?? 0, 2), "DAILY_ALLOWANCE mismatch");
            Assert.AreEqual(Math.Round(LUONG_CONG_THUCTE ?? 0, 2), Math.Round(other.LUONG_CONG_THUCTE ?? 0, 2), "LUONG_CONG_THUCTE mismatch");
            Assert.AreEqual(Math.Round(PHUCAP_CONG_THUCTE ?? 0, 2), Math.Round(other.PHUCAP_CONG_THUCTE ?? 0, 2), "PHUCAP_CONG_THUCTE mismatch");
            Assert.AreEqual(TIEN_TANGCA, other.TIEN_TANGCA, "TIEN_TANGCA mismatch");
            Assert.AreEqual(TIEN_CHUYENCAN, other.TIEN_CHUYENCAN, "TIEN_CHUYENCAN mismatch");
            Assert.AreEqual(TIEN_AN_CA, other.TIEN_AN_CA, "TIEN_AN_CA mismatch");
            Assert.AreEqual(KHOAN_CONG_KHAC, other.KHOAN_CONG_KHAC, "KHOAN_CONG_KHAC mismatch");
            Assert.AreEqual(TIEN_BHXH_TRICH, other.TIEN_BHXH_TRICH, "TIEN_BHXH_TRICH mismatch");
            Assert.AreEqual(TIEN_TAMUNG, other.TIEN_TAMUNG, "TIEN_TAMUNG mismatch");
            Assert.AreEqual(KHOAN_TRU_KHAC, other.KHOAN_TRU_KHAC, "KHOAN_TRU_KHAC mismatch");
            Assert.AreEqual(Math.Round(THUC_LINH ?? 0, 2), Math.Round(other.THUC_LINH ?? 0, 2), "THUC_LINH mismatch");
            Assert.AreEqual(ComputeNormalizedHash(), other.ComputeNormalizedHash(), "Normalized SHA-256 fingerprint mismatch");
        }
    }

    [TestFixture]
    public class DiagnosticIdbl1934Tests
    {
        private const string ConnectionString = "DATA SOURCE=localhost:1521/orcl;PASSWORD=hr;USER ID=HR";

        [Test]
        public void Inspect_Idbl_1934_And_Manv_2453()
        {
            using (var db = new MyEntities())
            {
                Console.WriteLine("=== QUERYING ALL ROWS OF TB_PHUCAP ===");
                var allPcs = db.Database.SqlQuery<string>(
                    "SELECT 'IDPC=' || IDPC || ', TENPC=' || TENPC FROM TB_PHUCAP ORDER BY IDPC"
                ).ToList();
                foreach (var p in allPcs) Console.WriteLine(p);

                Console.WriteLine("\n=== CHECKING EXISTING TABLES IN HR SCHEMA ===");
                var tbls = db.Database.SqlQuery<string>(
                    "SELECT TABLE_NAME FROM USER_TABLES ORDER BY TABLE_NAME"
                ).ToList();
                foreach (var t in tbls) Console.WriteLine($"Table: {t}");

                Console.WriteLine("\n=== DISTINCT MAKYCONG IN TB_KYCONG ===");
                var kycongs = db.Database.SqlQuery<decimal>("SELECT MAKYCONG FROM TB_KYCONG ORDER BY MAKYCONG").ToList();
                foreach (var kc in kycongs) Console.WriteLine($"TB_KYCONG: {kc}");

                Console.WriteLine("\n=== COUNT PER MAKYCONG IN TB_BANGLUONG ===");
                var counts = db.Database.SqlQuery<string>("SELECT 'MAKYCONG=' || MAKYCONG || ', COUNT=' || COUNT(*) FROM TB_BANGLUONG GROUP BY MAKYCONG ORDER BY MAKYCONG").ToList();
                foreach (var c in counts) Console.WriteLine($"TB_BANGLUONG: {c}");

                Console.WriteLine("=== QUERYING IDBL = 1934 BASELINE ===");
                var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.IDBL == 1934);
                Assert.IsNotNull(bl, "IDBL = 1934 must exist");
                Assert.AreEqual(2453, bl.MANV);
                Assert.AreEqual(202601, bl.MAKYCONG);
                Assert.AreEqual(1, bl.THANG);
                Assert.AreEqual(2026, bl.NAM);
                Assert.AreEqual(26, bl.CONG_CHUAN);
                Assert.AreEqual(27, bl.CONG_THUCTE);
                Assert.AreEqual(0, bl.CONG_LAMDEM);
                Assert.AreEqual(296153.85, Math.Round(bl.DAILY_RATE.Value, 2));
                Assert.AreEqual(126923.08, Math.Round(bl.DAILY_ALLOWANCE.Value, 2));
                Assert.AreEqual(7996153.85, Math.Round(bl.LUONG_CONG_THUCTE.Value, 2));
                Assert.AreEqual(3426923.08, Math.Round(bl.PHUCAP_CONG_THUCTE.Value, 2));
                Assert.AreEqual(0, bl.TIEN_TANGCA);
                Assert.AreEqual(660000, bl.TIEN_CHUYENCAN);
                Assert.AreEqual(0, bl.TIEN_AN_CA);
                Assert.AreEqual(0, bl.KHOAN_CONG_KHAC);
                Assert.AreEqual(808500, bl.TIEN_BHXH_TRICH);
                Assert.AreEqual(0, bl.TIEN_TAMUNG);
                Assert.AreEqual(13729, bl.KHOAN_TRU_KHAC);
                Assert.AreEqual(11260847.92, Math.Round(bl.THUC_LINH.Value, 2));
                Console.WriteLine("IDBL = 1934 verified successfully against full regression fixture!");
            }
        }

        [Test]
        public void Test_01_Partial_Migration_Guard_Detects_And_Blocks()
        {
            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                // Check how many V1_16 tables currently exist
                using (var cmd = new OracleCommand(@"
                    SELECT COUNT(*) FROM USER_TABLES 
                    WHERE TABLE_NAME IN (
                        'TB_PAYROLL_CALCULATION_RUN', 'TB_CHINH_SACH_LUONG', 'TB_CHINH_SACH_BHXH',
                        'TB_CHINH_SACH_BHXH_VUNG', 'TB_CHINH_SACH_CONG_DOAN', 'TB_THUE_TNCN_CHINH_SACH',
                        'TB_THUE_TNCN_BAC', 'TB_NHANVIEN_BAOHIEM_THAM_GIA', 'TB_NHANVIEN_CONG_DOAN_THAM_GIA',
                        'TB_NHANVIEN_THUE', 'TB_NGUOI_PHU_THUOC', 'TB_BANGLUONG_OT_COMPLIANCE',
                        'TB_BANGLUONG_CT', 'TB_BANGLUONG_CT_SOURCE', 'TB_BANGLUONG_BAOHIEM',
                        'TB_BANGLUONG_CONG_DOAN', 'TB_BANGLUONG_THUE_CT', 'TB_QUYET_TOAN_THUE_NAM',
                        'TB_QUYET_TOAN_THUE_NAM_CT'
                    )", conn))
                {
                    int partialCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"Current V1_16 table count in database: {partialCount}/19");

                    if (partialCount > 0 && partialCount < 19)
                    {
                        string solutionRoot = GetSolutionRoot();
                        string scriptPath = Path.Combine(solutionRoot, "database", "migrations", "V1_16__payroll_production_policies_and_itemized_details.sql");
                        string scriptText = File.ReadAllText(scriptPath);
                        var commands = ParseOracleScript(scriptText);

                        // Executing command #1 (Preflight) MUST throw ORA-20000
                        var ex = Assert.Throws<OracleException>(() =>
                        {
                            using (var preflightCmd = new OracleCommand(commands[0], conn))
                            {
                                preflightCmd.ExecuteNonQuery();
                            }
                        });

                        Console.WriteLine($"Preflight successfully blocked partial rerun with: {ex.Message}");
                        StringAssert.Contains("ORA-20000", ex.Message);
                        StringAssert.Contains("Partial V1_16 migration detected", ex.Message);
                    }
                    else
                    {
                        Console.WriteLine("Table count is not in partial state (0 or 19). Skipping partial guard assertion.");
                    }
                }
            }
        }

        [Test]
        public void Test_02_Clean_Migration_Execution_And_Postflight_Verification()
        {
            string solutionRoot = GetSolutionRoot();
            string scriptPath = Path.Combine(solutionRoot, "database", "migrations", "V1_16__payroll_production_policies_and_itemized_details.sql");
            Assert.IsTrue(File.Exists(scriptPath), $"Migration script not found at: {scriptPath}");

            string scriptText = File.ReadAllText(scriptPath);

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                // 1. Teardown any partial V1_16 tables to start completely clean (0/19 tables)
                Console.WriteLine("=== STEP 0: TEARDOWN V1_16 OBJECTS FOR CLEAN REPRODUCIBLE MIGRATION ===");
                Teardown_V1_16_Objects(conn);

                // 2. Capture Pre-Migration Fingerprint of IDBL = 1934
                Console.WriteLine("\n=== STEP 1: CAPTURING PRE-MIGRATION FINGERPRINT (IDBL=1934) ===");
                var preFingerprint = Idbl1934Fingerprint.Capture(conn);
                string preHash = preFingerprint.ComputeNormalizedHash();
                Console.WriteLine($"Pre-migration SHA-256 fingerprint: {preHash}");

                // 3. Parse and Execute Migration Script
                Console.WriteLine("\n=== STEP 2: PARSING & EXECUTING V1_16 MIGRATION SCRIPT ===");
                var commands = ParseOracleScript(scriptText);
                Console.WriteLine($"Total parsed commands to execute: {commands.Count}");

                int executed = 0;
                foreach (var cmdText in commands)
                {
                    executed++;
                    using (var cmd = new OracleCommand(cmdText, conn))
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"FAILED on command #{executed}:\n{cmdText}\n");
                            throw new InvalidOperationException($"Error executing command #{executed}: {ex.Message}", ex);
                        }
                    }
                }
                Console.WriteLine($"Successfully executed all {executed} statements/blocks!");

                // 4. Capture Post-Migration Fingerprint of IDBL = 1934
                Console.WriteLine("\n=== STEP 3: CAPTURING POST-MIGRATION FINGERPRINT (IDBL=1934) ===");
                var postFingerprint = Idbl1934Fingerprint.Capture(conn);
                string postHash = postFingerprint.ComputeNormalizedHash();
                Console.WriteLine($"Post-migration SHA-256 fingerprint: {postHash}");

                // 5. Assert Exact Immutability
                preFingerprint.AssertExactMatch(postFingerprint);
                Assert.AreEqual(preHash, postHash, "Legacy row IDBL=1934 financial fingerprint MUST be 100% immutable!");
                Console.WriteLine("IDBL=1934 fingerprint is 100% byte-for-byte identical before and after migration!");

                // 6. Verify Legacy Snapshot Flags
                using (var cmd = new OracleCommand("SELECT IS_LEGACY, TRANG_THAI FROM TB_BANGLUONG WHERE IDBL = 1934", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    Assert.IsTrue(reader.Read(), "IDBL=1934 must exist");
                    int isLegacy = Convert.ToInt32(reader["IS_LEGACY"]);
                    string trangThai = reader["TRANG_THAI"].ToString();
                    Assert.AreEqual(1, isLegacy, "IDBL=1934 must have IS_LEGACY = 1");
                    Assert.AreEqual("LEGACY_READONLY", trangThai, "IDBL=1934 must have TRANG_THAI = 'LEGACY_READONLY'");
                }

                // 7. Verify All 19 Tables in USER_TABLES
                var expectedTables = new[]
                {
                    "TB_PAYROLL_CALCULATION_RUN",
                    "TB_CHINH_SACH_LUONG",
                    "TB_CHINH_SACH_BHXH",
                    "TB_CHINH_SACH_BHXH_VUNG",
                    "TB_CHINH_SACH_CONG_DOAN",
                    "TB_THUE_TNCN_CHINH_SACH",
                    "TB_THUE_TNCN_BAC",
                    "TB_NHANVIEN_BAOHIEM_THAM_GIA",
                    "TB_NHANVIEN_CONG_DOAN_THAM_GIA",
                    "TB_NHANVIEN_THUE",
                    "TB_NGUOI_PHU_THUOC",
                    "TB_BANGLUONG_OT_COMPLIANCE",
                    "TB_BANGLUONG_CT",
                    "TB_BANGLUONG_CT_SOURCE",
                    "TB_BANGLUONG_BAOHIEM",
                    "TB_BANGLUONG_CONG_DOAN",
                    "TB_BANGLUONG_THUE_CT",
                    "TB_QUYET_TOAN_THUE_NAM",
                    "TB_QUYET_TOAN_THUE_NAM_CT"
                };

                Console.WriteLine("\n=== STEP 4: VERIFYING ALL 19 NEW TABLES ===");
                foreach (var tbl in expectedTables)
                {
                    using (var cmd = new OracleCommand($"SELECT COUNT(*) FROM USER_TABLES WHERE TABLE_NAME = '{tbl}'", conn))
                    {
                        int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert.AreEqual(1, cnt, $"Table {tbl} must exist in USER_TABLES");
                    }
                }
                Console.WriteLine("All 19 new tables verified successfully!");

                // 8. Verify All 6 Triggers VALID
                var expectedTriggers = new[]
                {
                    "TRG_LOCK_LUONG_POLICY",
                    "TRG_LOCK_BHXH_POLICY",
                    "TRG_LOCK_BHXH_VUNG",
                    "TRG_LOCK_CONGDOAN_POLICY",
                    "TRG_LOCK_THUE_POLICY",
                    "TRG_LOCK_THUE_BAC"
                };
                Console.WriteLine("\n=== STEP 5: VERIFYING ALL 6 VALID TRIGGERS ===");
                foreach (var trg in expectedTriggers)
                {
                    using (var cmd = new OracleCommand($"SELECT STATUS FROM USER_OBJECTS WHERE OBJECT_TYPE = 'TRIGGER' AND OBJECT_NAME = '{trg}'", conn))
                    {
                        var status = cmd.ExecuteScalar()?.ToString();
                        Assert.AreEqual("VALID", status, $"Trigger {trg} must have status VALID");
                    }
                }
                Console.WriteLine("All 6 triggers verified as VALID!");

                // 9. Verify Sequences & Safety (NEXTVAL > MAX(ID))
                Console.WriteLine("\n=== STEP 6: VERIFYING ALL 19 SEQUENCES & ADVANCEMENT ===");
                var expectedSequences = new[]
                {
                    "SEQ_PAYROLL_CALC_RUN",
                    "SEQ_CHINH_SACH_LUONG",
                    "SEQ_CHINH_SACH_BHXH",
                    "SEQ_CHINH_SACH_BHXH_VUNG",
                    "SEQ_CHINH_SACH_CONG_DOAN",
                    "SEQ_THUE_TNCN_CHINH_SACH",
                    "SEQ_THUE_TNCN_BAC",
                    "SEQ_NV_BAOHIEM_TG",
                    "SEQ_NV_CONGDOAN_TG",
                    "SEQ_NV_THUE",
                    "SEQ_NGUOI_PHU_THUOC",
                    "SEQ_BANGLUONG_OT_COMPLIANCE",
                    "SEQ_BANGLUONG_CT",
                    "SEQ_BANGLUONG_CT_SOURCE",
                    "SEQ_BANGLUONG_BAOHIEM",
                    "SEQ_BANGLUONG_CONG_DOAN",
                    "SEQ_BANGLUONG_THUE_CT",
                    "SEQ_QUYET_TOAN_THUE_NAM",
                    "SEQ_QUYET_TOAN_THUE_NAM_CT"
                };
                foreach (var seq in expectedSequences)
                {
                    using (var cmd = new OracleCommand($"SELECT COUNT(*) FROM USER_SEQUENCES WHERE SEQUENCE_NAME = '{seq}'", conn))
                    {
                        int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                        Assert.AreEqual(1, cnt, $"Sequence {seq} must exist in USER_SEQUENCES");
                    }
                }

                // Verify NEXTVAL > MAX(ID)
                using (var cmd = new OracleCommand("SELECT MAX(ID) FROM TB_CHINH_SACH_BHXH", conn))
                {
                    decimal maxId = Convert.ToDecimal(cmd.ExecuteScalar());
                    using (var seqCmd = new OracleCommand("SELECT SEQ_CHINH_SACH_BHXH.NEXTVAL FROM DUAL", conn))
                    {
                        decimal nextVal = Convert.ToDecimal(seqCmd.ExecuteScalar());
                        Assert.Greater(nextVal, maxId, "SEQ_CHINH_SACH_BHXH NEXTVAL must be greater than MAX(ID)");
                    }
                }

                // 10. Verify Policy Seeds & Tax Brackets
                Console.WriteLine("\n=== STEP 7: VERIFYING SEEDED POLICIES & BRACKETS ===");
                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM TB_CHINH_SACH_BHXH", conn))
                {
                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.AreEqual(2, cnt, "Expected exactly 2 BHXH policies (H1, H2)");
                }

                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM TB_CHINH_SACH_BHXH_VUNG", conn))
                {
                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.AreEqual(8, cnt, "Expected exactly 8 BHXH regional rate rows (4 regions * 2 policies)");
                }

                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM TB_THUE_TNCN_BAC", conn))
                {
                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.AreEqual(10, cnt, "Expected exactly 10 tax brackets");
                }

                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM TB_THUE_TNCN_BAC WHERE PERIOD_TYPE = 'MONTH'", conn))
                {
                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.AreEqual(5, cnt, "Expected exactly 5 MONTH tax brackets");
                }

                using (var cmd = new OracleCommand("SELECT COUNT(*) FROM TB_THUE_TNCN_BAC WHERE PERIOD_TYPE = 'YEAR'", conn))
                {
                    int cnt = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.AreEqual(5, cnt, "Expected exactly 5 YEAR tax brackets");
                }

                // 11. Verify SCHEMA_VERSION Registry
                Console.WriteLine("\n=== STEP 8: VERIFYING SCHEMA_VERSION RECORD ===");
                using (var cmd = new OracleCommand("SELECT SUCCESS FROM SCHEMA_VERSION WHERE VERSION = '1.16'", conn))
                {
                    var success = Convert.ToInt32(cmd.ExecuteScalar());
                    Assert.AreEqual(1, success, "SCHEMA_VERSION for 1.16 must be recorded with SUCCESS = 1");
                }
                Console.WriteLine("SCHEMA_VERSION 1.16 verified successfully!");

                Console.WriteLine("\n=== ALL POSTFLIGHT & IMMUTABILITY CHECKS PASSED WITH 100% SUCCESS ===");
            }
        }

        [Test]
        public void Test_03_Rerun_Guard_Blocks_Second_Execution()
        {
            string solutionRoot = GetSolutionRoot();
            string scriptPath = Path.Combine(solutionRoot, "database", "migrations", "V1_16__payroll_production_policies_and_itemized_details.sql");
            string scriptText = File.ReadAllText(scriptPath);
            var commands = ParseOracleScript(scriptText);

            using (var conn = new OracleConnection(ConnectionString))
            {
                conn.Open();

                // Executing command #1 (Preflight) when V1_16 is installed MUST throw ORA-20004
                var ex = Assert.Throws<OracleException>(() =>
                {
                    using (var preflightCmd = new OracleCommand(commands[0], conn))
                    {
                        preflightCmd.ExecuteNonQuery();
                    }
                });

                Console.WriteLine($"Preflight successfully blocked rerun with: {ex.Message}");
                StringAssert.Contains("ORA-20004", ex.Message);
                StringAssert.Contains("ALREADY INSTALLED", ex.Message);
            }
        }

        private static string GetSolutionRoot()
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            while (!string.IsNullOrEmpty(root) && !File.Exists(Path.Combine(root, "HRMS.sln")))
            {
                root = Path.GetDirectoryName(root);
            }
            return root;
        }

        private static void Teardown_V1_16_Objects(OracleConnection conn)
        {
            var triggers = new[] {
                "TRG_LOCK_LUONG_POLICY", "TRG_LOCK_BHXH_POLICY", "TRG_LOCK_BHXH_VUNG",
                "TRG_LOCK_CONGDOAN_POLICY", "TRG_LOCK_THUE_POLICY", "TRG_LOCK_THUE_BAC"
            };
            foreach (var trg in triggers)
            {
                try { using (var cmd = new OracleCommand($"DROP TRIGGER {trg}", conn)) cmd.ExecuteNonQuery(); } catch { }
            }

            var tables = new[] {
                "TB_QUYET_TOAN_THUE_NAM_CT", "TB_QUYET_TOAN_THUE_NAM", "TB_BANGLUONG_THUE_CT",
                "TB_BANGLUONG_CONG_DOAN", "TB_BANGLUONG_BAOHIEM", "TB_BANGLUONG_CT_SOURCE",
                "TB_BANGLUONG_CT", "TB_BANGLUONG_OT_COMPLIANCE", "TB_NGUOI_PHU_THUOC",
                "TB_NHANVIEN_THUE", "TB_NHANVIEN_CONG_DOAN_THAM_GIA", "TB_NHANVIEN_BAOHIEM_THAM_GIA",
                "TB_THUE_TNCN_BAC", "TB_THUE_TNCN_CHINH_SACH", "TB_CHINH_SACH_CONG_DOAN",
                "TB_CHINH_SACH_BHXH_VUNG", "TB_CHINH_SACH_BHXH", "TB_CHINH_SACH_LUONG",
                "TB_PAYROLL_CALCULATION_RUN"
            };
            foreach (var tbl in tables)
            {
                try { using (var cmd = new OracleCommand($"DROP TABLE {tbl} CASCADE CONSTRAINTS PURGE", conn)) cmd.ExecuteNonQuery(); } catch { }
            }

            var constraints = new[] {
                ("TB_BANGCONG_CHITIET", "UQ_BCCT_ID_MANV"),
                ("TB_TANGCA", "UQ_TC_ID_MANV"),
                ("TB_NHANVIEN_PHUCAP", "UQ_NVPC_ID_MANV"),
                ("TB_KHENTHUONG_KYLUAT", "UQ_KTKL_ID_MANV"),
                ("TB_UNGLUONG", "UQ_UL_ID_MANV"),
                ("TB_BANGLUONG", "FK_BL_PCR"),
                ("TB_BANGLUONG", "FK_BL_POL_LUONG"),
                ("TB_BANGLUONG", "FK_BL_POL_BHXH"),
                ("TB_BANGLUONG", "FK_BL_POL_CD"),
                ("TB_BANGLUONG", "FK_BL_POL_THUE"),
                ("TB_BANGLUONG", "FK_BL_NV_BH"),
                ("TB_BANGLUONG", "FK_BL_NV_CD"),
                ("TB_BANGLUONG", "FK_BL_NV_THUE"),
                ("TB_BANGLUONG", "UQ_BANGLUONG_MANV_KYCONG"),
                ("TB_BANGLUONG", "UQ_BANGLUONG_ID_MANV"),
                ("TB_BANGLUONG", "CK_BL_LEGACY")
            };
            foreach (var (tbl, c) in constraints)
            {
                try { using (var cmd = new OracleCommand($"ALTER TABLE {tbl} DROP CONSTRAINT {c} CASCADE DROP INDEX", conn)) cmd.ExecuteNonQuery(); } catch { }
                try { using (var cmd = new OracleCommand($"ALTER TABLE {tbl} DROP CONSTRAINT {c} CASCADE", conn)) cmd.ExecuteNonQuery(); } catch { }
                try { using (var cmd = new OracleCommand($"DROP INDEX {c}", conn)) cmd.ExecuteNonQuery(); } catch { }
            }

            var sequences = new[] {
                "SEQ_PAYROLL_CALC_RUN", "SEQ_CHINH_SACH_LUONG", "SEQ_CHINH_SACH_BHXH",
                "SEQ_CHINH_SACH_BHXH_VUNG", "SEQ_CHINH_SACH_CONG_DOAN", "SEQ_THUE_TNCN_CHINH_SACH",
                "SEQ_THUE_TNCN_BAC", "SEQ_NV_BAOHIEM_TG", "SEQ_NV_CONGDOAN_TG",
                "SEQ_NV_THUE", "SEQ_NGUOI_PHU_THUOC", "SEQ_BANGLUONG_OT_COMPLIANCE",
                "SEQ_BANGLUONG_CT", "SEQ_BANGLUONG_CT_SOURCE", "SEQ_BANGLUONG_BAOHIEM",
                "SEQ_BANGLUONG_CONG_DOAN", "SEQ_BANGLUONG_THUE_CT", "SEQ_QUYET_TOAN_THUE_NAM",
                "SEQ_QUYET_TOAN_THUE_NAM_CT"
            };
            foreach (var seq in sequences)
            {
                try { using (var cmd = new OracleCommand($"DROP SEQUENCE {seq}", conn)) cmd.ExecuteNonQuery(); } catch { }
            }

            try { using (var cmd = new OracleCommand("DELETE FROM SCHEMA_VERSION WHERE VERSION = '1.16' OR SCRIPT LIKE '%V1_16%'", conn)) cmd.ExecuteNonQuery(); } catch { }

            var blCols = new[] {
                "IS_LEGACY", "TRANG_THAI", "POLICY_LUONG_ID", "POLICY_BHXH_ID", "POLICY_CONGDOAN_ID",
                "POLICY_THUE_ID", "PROFILE_BH_ID", "PROFILE_CD_ID", "PROFILE_THUE_ID", "VUNG_LUONG",
                "LUONG_TOI_THIEU_VUNG", "MUC_THAM_CHIEU_BH", "LUONG_DONG_BHXH", "TIEN_BHYT_NLD",
                "TIEN_BHTN_NLD", "TIEN_BHXH_NSDLD", "TIEN_BHYT_NSDLD", "TIEN_BHTN_NSDLD",
                "TIEN_TNLD_BNN_NSDLD", "TIEN_DOAN_PHI_NLD", "TIEN_KINH_PHI_CD_NSDLD",
                "SO_NGUOI_PHU_THUOC", "GIAM_TRU_BAN_THAN", "GIAM_TRU_PHU_THUOC", "GIAM_TRU_BAO_HIEM",
                "TONG_THU_NHAP_CHIU_THUE", "THU_NHAP_TINH_THUE", "TONG_CHI_PHI_NSDLD",
                "RUN_ID", "CALCULATED_AT", "APPROVED_AT", "APPROVED_BY"
            };
            foreach (var col in blCols)
            {
                try { using (var cmd = new OracleCommand($"ALTER TABLE TB_BANGLUONG DROP COLUMN {col}", conn)) cmd.ExecuteNonQuery(); } catch { }
            }
        }

        private static List<string> ParseOracleScript(string scriptText)
        {
            var commands = new List<string>();
            var lines = scriptText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            var currentBlock = new StringBuilder();
            bool inPlSql = false;

            foreach (var rawLine in lines)
            {
                string trimmedLine = rawLine.Trim();

                if (trimmedLine == "/")
                {
                    if (currentBlock.Length > 0)
                    {
                        string cmd = currentBlock.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(cmd))
                        {
                            commands.Add(cmd);
                        }
                        currentBlock.Clear();
                    }
                    inPlSql = false;
                    continue;
                }

                if (!inPlSql && trimmedLine.StartsWith("--"))
                {
                    continue;
                }

                if (!inPlSql)
                {
                    string upper = trimmedLine.ToUpperInvariant();
                    if (upper.StartsWith("DECLARE") || upper.StartsWith("BEGIN") || upper.StartsWith("CREATE OR REPLACE TRIGGER") || upper.StartsWith("CREATE TRIGGER"))
                    {
                        inPlSql = true;
                    }
                }

                if (inPlSql)
                {
                    currentBlock.AppendLine(rawLine);
                }
                else
                {
                    currentBlock.AppendLine(rawLine);

                    string accumulated = currentBlock.ToString().Trim();
                    string cleaned = Regex.Replace(accumulated, @"--[^\r\n]*", "").Trim();
                    if (cleaned.EndsWith(";"))
                    {
                        string toExec = cleaned.Substring(0, cleaned.Length - 1).Trim();
                        if (!string.IsNullOrWhiteSpace(toExec))
                        {
                            commands.Add(toExec);
                        }
                        currentBlock.Clear();
                    }
                }
            }

            if (currentBlock.Length > 0)
            {
                string remaining = currentBlock.ToString().Trim();
                string cleaned = Regex.Replace(remaining, @"--[^\r\n]*", "").Trim();
                if (cleaned.EndsWith(";")) cleaned = cleaned.Substring(0, cleaned.Length - 1).Trim();
                if (cleaned == "/") cleaned = "";
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    commands.Add(cleaned);
                }
            }

            return commands;
        }
    }
}
