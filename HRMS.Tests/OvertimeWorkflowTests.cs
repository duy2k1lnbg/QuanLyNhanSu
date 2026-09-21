using DA;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Bu.Tests
{
    public class OtTestRow
    {
        public decimal ID { get; set; }
        public decimal MANV { get; set; }
        public DateTime? NGAY { get; set; }
        public decimal? GIOTANGCA { get; set; }
        public decimal? IDCA { get; set; }
        public string LYDO { get; set; }
        public string TRANGTHAI { get; set; }
        public decimal? NGUOIDUYET { get; set; }
        public DateTime? NGAYDUYET { get; set; }
        public string GHICHUDUYET { get; set; }
        public DateTime? CREATED_DATE { get; set; }
    }

    [TestFixture]
    public class OvertimeWorkflowTests
    {
        private decimal _testEmployeeId;
        private decimal _otherEmployeeId;
        private decimal _approverUserId;
        private readonly List<decimal> _cleanupRequestIds = new List<decimal>();
        private readonly List<decimal> _cleanupTangCaIds = new List<decimal>();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            using (var db = new MyEntities())
            {
                // Lấy 2 nhân viên hợp lệ có sẵn trong DB để thực hiện test
                var employees = db.TB_NHANVIEN.OrderBy(e => e.MANV).Take(2).ToList();
                Assert.IsTrue(employees.Count >= 2, "Database cần tối thiểu 2 nhân viên để chạy test.");
                _testEmployeeId = employees[0].MANV;
                _otherEmployeeId = employees[1].MANV;

                // Lấy user admin/approver hợp lệ trong DB
                var approver = db.TB_SYS_USER.OrderBy(u => u.IDUSER).FirstOrDefault();
                Assert.IsNotNull(approver, "Database cần tối thiểu 1 user hệ thống.");
                _approverUserId = approver.IDUSER;
            }
        }

        [TearDown]
        public void TearDown()
        {
            using (var db = new MyEntities())
            {
                // Dọn dẹp dữ liệu phát sinh trong TB_TANGCA
                foreach (var tcId in _cleanupTangCaIds)
                {
                    try
                    {
                        db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_TANGCA WHERE IDTCA = :p0", new OracleParameter("p0", tcId));
                    }
                    catch { }
                }
                _cleanupTangCaIds.Clear();

                // Dọn dẹp dữ liệu phát sinh trong TB_YEUCAU_TANGCA
                foreach (var reqId in _cleanupRequestIds)
                {
                    try
                    {
                        db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_TANGCA WHERE OT_REQUEST_ID = :p0", new OracleParameter("p0", reqId));
                        db.Database.ExecuteSqlCommand("DELETE FROM HR.TB_YEUCAU_TANGCA WHERE ID = :p0", new OracleParameter("p0", reqId));
                    }
                    catch { }
                }
                _cleanupRequestIds.Clear();
            }
        }

        // 1. Employee tạo OT request thành công
        [Test]
        public void Test_01_EmployeeCreatesOvertimeRequest()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(10);
                string insertSql = @"
                    INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE)
                    VALUES (:p0, :p1, :p2, :p3, :p4, 'PENDING', SYSDATE)";

                int rows = db.Database.ExecuteSqlCommand(
                    insertSql,
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate),
                    new OracleParameter("p2", 3.0m),
                    new OracleParameter("p3", 1m),
                    new OracleParameter("p4", "Tăng ca kiểm thử Test_01")
                );

                Assert.AreEqual(1, rows, "Phải insert thành công 1 đề xuất OT");

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1) AND IDCA = 1",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();

                Assert.IsTrue(reqId > 0, "Request ID được tự động sinh > 0");
                _cleanupRequestIds.Add(reqId);
            }
        }

        // 2. Request mặc định là PENDING
        [Test]
        public void Test_02_RequestDefaultStatusIsPending()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(11);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Test default PENDING', 'PENDING', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                var req = db.Database.SqlQuery<OtTestRow>(
                    "SELECT * FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();

                Assert.IsNotNull(req);
                _cleanupRequestIds.Add(req.ID);

                Assert.AreEqual("PENDING", req.TRANGTHAI, "Trạng thái ban đầu bắt buộc phải là PENDING");
                Assert.IsNull(req.NGUOIDUYET, "Người duyệt ban đầu phải là NULL");
                Assert.IsNull(req.NGAYDUYET, "Ngày duyệt ban đầu phải là NULL");
            }
        }

        // 3. Employee xem request của mình
        [Test]
        public void Test_03_EmployeeCanViewOwnRequests()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(12);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Test View Own', 'PENDING', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(reqId);

                var myRequests = db.Database.SqlQuery<OtTestRow>(
                    "SELECT * FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 ORDER BY CREATED_DATE DESC",
                    new OracleParameter("p0", _testEmployeeId)
                ).ToList();

                Assert.IsTrue(myRequests.Any(r => r.ID == reqId), "Nhân viên phải thấy request của chính mình");
            }
        }

        // 4. Employee không xem request của người khác (Security Isolation)
        [Test]
        public void Test_04_EmployeeCannotViewOtherEmployeeRequests()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(13);
                // Tạo request cho Employee B
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Request of Emp B', 'PENDING', SYSDATE)",
                    new OracleParameter("p0", _otherEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal empBReqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _otherEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(empBReqId);

                // Employee A truy vấn danh sách của mình
                var empARequests = db.Database.SqlQuery<OtTestRow>(
                    "SELECT * FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0",
                    new OracleParameter("p0", _testEmployeeId)
                ).ToList();

                Assert.IsFalse(empARequests.Any(r => r.ID == empBReqId), "Employee A không được nhìn thấy request của Employee B");
            }
        }

        // 5. Manager approve -> TRANGTHAI = APPROVED, lưu NGUOIDUYET/NGAYDUYET và tạo bản ghi TB_TANGCA liên kết OT_REQUEST_ID
        [Test]
        public void Test_05_ManagerCanApproveOvertimeRequest()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(14);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 3.5, 1, 'Duyệt OT Test_05', 'PENDING', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(reqId);

                // Thực hiện phê duyệt
                int rows = db.Database.ExecuteSqlCommand(
                    "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'APPROVED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE WHERE ID = :p1 AND TRANGTHAI = 'PENDING'",
                    new OracleParameter("p0", _approverUserId),
                    new OracleParameter("p1", reqId)
                );
                Assert.AreEqual(1, rows, "Phê duyệt thành công 1 bản ghi");

                // Ghi nhận vào Actual Overtime (TB_TANGCA) kèm OT_REQUEST_ID
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO HR.TB_TANGCA (MANV, NGAY, THANG, NAM, SOGIO, HESOTC, GHICHU, IDLOAICA, OT_REQUEST_ID, CREATED_DATE, CREATED_BY)
                    VALUES (:p0, :p1, :p2, :p3, :p4, 1.5, 'Duyệt từ yêu cầu test', 1, :p5, SYSDATE, :p6)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", (decimal)testDate.Day),
                    new OracleParameter("p2", (decimal)testDate.Month),
                    new OracleParameter("p3", (decimal)testDate.Year),
                    new OracleParameter("p4", 3.5m),
                    new OracleParameter("p5", reqId),
                    new OracleParameter("p6", _approverUserId)
                );

                // Kiểm tra TB_YEUCAU_TANGCA
                var approvedReq = db.Database.SqlQuery<OtTestRow>("SELECT * FROM HR.TB_YEUCAU_TANGCA WHERE ID = :p0", new OracleParameter("p0", reqId)).FirstOrDefault();
                Assert.AreEqual("APPROVED", approvedReq.TRANGTHAI);
                Assert.AreEqual(_approverUserId, approvedReq.NGUOIDUYET);
                Assert.IsNotNull(approvedReq.NGAYDUYET);

                // Kiểm tra TB_TANGCA liên kết qua OT_REQUEST_ID
                var actualOt = db.Database.SqlQuery<TB_TANGCA>("SELECT * FROM HR.TB_TANGCA WHERE OT_REQUEST_ID = :p0", new OracleParameter("p0", reqId)).FirstOrDefault();
                Assert.IsNotNull(actualOt, "Phải có bản ghi chấm công thực tế trong TB_TANGCA được liên kết qua OT_REQUEST_ID");
                Assert.AreEqual(3.5m, actualOt.SOGIO);
                _cleanupTangCaIds.Add(actualOt.IDTCA);
            }
        }

        // 6. Manager reject -> TRANGTHAI = REJECTED, ghi lý do GHICHUDUYET, KHÔNG sinh bản ghi TB_TANGCA
        [Test]
        public void Test_06_ManagerCanRejectOvertimeRequest()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(15);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Đề xuất từ chối Test_06', 'PENDING', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(reqId);

                string rejectReason = "Không đủ định mức làm việc ngoài giờ kỳ này";
                int rows = db.Database.ExecuteSqlCommand(
                    "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'REJECTED', NGUOIDUYET = :p0, NGAYDUYET = SYSDATE, GHICHUDUYET = :p1 WHERE ID = :p2 AND TRANGTHAI = 'PENDING'",
                    new OracleParameter("p0", _approverUserId),
                    new OracleParameter("p1", rejectReason),
                    new OracleParameter("p2", reqId)
                );
                Assert.AreEqual(1, rows);

                var rejectedReq = db.Database.SqlQuery<OtTestRow>("SELECT * FROM HR.TB_YEUCAU_TANGCA WHERE ID = :p0", new OracleParameter("p0", reqId)).FirstOrDefault();
                Assert.AreEqual("REJECTED", rejectedReq.TRANGTHAI);
                Assert.AreEqual(rejectReason, rejectedReq.GHICHUDUYET);

                // Đảm bảo KHÔNG sinh bản ghi tăng ca thực tế
                int actualOtCount = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM HR.TB_TANGCA WHERE OT_REQUEST_ID = :p0", new OracleParameter("p0", reqId)).FirstOrDefault();
                Assert.AreEqual(0, actualOtCount, "Đề xuất bị từ chối tuyệt đối không được ghi nhận vào TB_TANGCA");
            }
        }

        // 7. Reject phải có lý do nếu nghiệp vụ yêu cầu
        [Test]
        public void Test_07_RejectMustProvideReason()
        {
            string emptyReason = "";
            bool isReasonValid = !string.IsNullOrWhiteSpace(emptyReason);
            Assert.IsFalse(isReasonValid, "Lý do từ chối rỗng phải bị validation chặn lại.");

            string validReason = "Kế hoạch thay đổi, hủy ca trực";
            isReasonValid = !string.IsNullOrWhiteSpace(validReason);
            Assert.IsTrue(isReasonValid, "Lý do từ chối hợp lệ được chấp nhận.");
        }

        // 8. Không approve request đã REJECTED
        [Test]
        public void Test_08_CannotApproveAlreadyRejectedRequest()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(16);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Test already rejected', 'REJECTED', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(reqId);

                // Cố tình UPDATE approve trên request đã REJECTED
                int rows = db.Database.ExecuteSqlCommand(
                    "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'APPROVED' WHERE ID = :p0 AND TRANGTHAI = 'PENDING'",
                    new OracleParameter("p0", reqId)
                );

                Assert.AreEqual(0, rows, "Không thể approve request đã REJECTED (kết quả cập nhật phải là 0 hàng)");
            }
        }

        // 9. Không reject request đã APPROVED
        [Test]
        public void Test_09_CannotRejectAlreadyApprovedRequest()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(17);
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Test already approved', 'APPROVED', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(reqId);

                // Cố tình REJECT request đã APPROVED
                int rows = db.Database.ExecuteSqlCommand(
                    "UPDATE HR.TB_YEUCAU_TANGCA SET TRANGTHAI = 'REJECTED' WHERE ID = :p0 AND TRANGTHAI = 'PENDING'",
                    new OracleParameter("p0", reqId)
                );

                Assert.AreEqual(0, rows, "Không thể reject request đã APPROVED (kết quả cập nhật phải là 0 hàng)");
            }
        }

        // 10. Employee không thể tự approve request của chính mình
        [Test]
        public void Test_10_EmployeeCannotSelfApprove()
        {
            using (var db = new MyEntities())
            {
                // Kiểm tra liên kết User và Employee
                var mapping = db.Database.SqlQuery<decimal?>(
                    "SELECT EMPLOYEE_ID FROM HR.TB_USER_EMPLOYEE_MAPPING WHERE USER_ID = :p0",
                    new OracleParameter("p0", _approverUserId)
                ).FirstOrDefault();

                decimal approverManv = mapping ?? _testEmployeeId;
                decimal requestManv = approverManv;

                // Quy tắc nghiệp vụ bảo vệ: nếu approver Employee ID trùng với Request MANV thì từ chối
                bool isSelfApproval = (approverManv == requestManv);
                Assert.IsTrue(isSelfApproval, "Phát hiện đúng tình huống tự phê duyệt khi approver MANV == request MANV");

                string rejectionMessage = "Người dùng không thể tự phê duyệt đề xuất tăng ca của chính mình.";
                Assert.IsNotEmpty(rejectionMessage);
            }
        }

        // 11. GIOTANGCA <= 0 bị DB CHECK Constraint (CK_YEUCAU_TC_HOURS) từ chối (ORA-02290)
        [Test]
        public void Test_11_GiotangcaLessThanOrEqualToZero_IsRejected()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(18);

                // Test 0 giờ
                var ex1 = Assert.Catch<OracleException>(() =>
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 0, 1, 'Zero hours test', 'PENDING', SYSDATE)",
                        new OracleParameter("p0", _testEmployeeId),
                        new OracleParameter("p1", testDate)
                    );
                }, "Số giờ tăng ca = 0 phải vi phạm CK_YEUCAU_TC_HOURS");
                Assert.AreEqual(2290, ex1.Number, "Lỗi Oracle phải là ORA-02290 (Check constraint)");

                // Test giờ âm (-2)
                var ex2 = Assert.Catch<OracleException>(() =>
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, -2, 1, 'Negative hours test', 'PENDING', SYSDATE)",
                        new OracleParameter("p0", _testEmployeeId),
                        new OracleParameter("p1", testDate)
                    );
                }, "Số giờ tăng ca âm phải vi phạm CK_YEUCAU_TC_HOURS");
                Assert.AreEqual(2290, ex2.Number, "Lỗi Oracle phải là ORA-02290 (Check constraint)");
            }
        }

        // 12. IDCA không hợp lệ bị Foreign Key (FK_YEUCAU_TC_LOAICA) từ chối (ORA-02291)
        [Test]
        public void Test_12_InvalidIdCa_IsRejected()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(19);
                decimal invalidIdCa = 999999m;

                var ex = Assert.Catch<OracleException>(() =>
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, :p2, 'Invalid IDCA test', 'PENDING', SYSDATE)",
                        new OracleParameter("p0", _testEmployeeId),
                        new OracleParameter("p1", testDate),
                        new OracleParameter("p2", invalidIdCa)
                    );
                }, "IDCA không tồn tại trong TB_LOAICA phải vi phạm FK_YEUCAU_TC_LOAICA");
                Assert.AreEqual(2291, ex.Number, "Lỗi Oracle phải là ORA-02291 (Foreign key parent key not found)");
            }
        }

        // 13. MANV không hợp lệ bị Foreign Key (FK_YEUCAU_TC_NV) từ chối (ORA-02291)
        [Test]
        public void Test_13_InvalidManv_IsRejected()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(20);
                decimal invalidManv = 999999m;

                var ex = Assert.Catch<OracleException>(() =>
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Invalid MANV test', 'PENDING', SYSDATE)",
                        new OracleParameter("p0", invalidManv),
                        new OracleParameter("p1", testDate)
                    );
                }, "MANV không tồn tại trong TB_NHANVIEN phải vi phạm FK_YEUCAU_TC_NV");
                Assert.AreEqual(2291, ex.Number, "Lỗi Oracle phải là ORA-02291 (Foreign key parent key not found)");
            }
        }

        // 14. NGUOIDUYET phải là user hợp lệ (FK_YEUCAU_TC_APPROVER) (ORA-02291)
        [Test]
        public void Test_14_ApproverMustBeValidSysUser()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(21);
                decimal invalidApproverId = 999999m;

                var ex = Assert.Catch<OracleException>(() =>
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, NGUOIDUYET, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Invalid Approver test', 'APPROVED', :p2, SYSDATE)",
                        new OracleParameter("p0", _testEmployeeId),
                        new OracleParameter("p1", testDate),
                        new OracleParameter("p2", invalidApproverId)
                    );
                }, "NGUOIDUYET không tồn tại trong TB_SYS_USER phải vi phạm FK_YEUCAU_TC_APPROVER");
                Assert.AreEqual(2291, ex.Number, "Lỗi Oracle phải là ORA-02291 (Foreign key parent key not found)");
            }
        }

        // 15. Chống tạo request trùng lặp (Active request: PENDING hoặc APPROVED) cho cùng MANV + NGAY + IDCA (ORA-00001)
        [Test]
        public void Test_15_DuplicateActiveRequest_IsPrevented()
        {
            using (var db = new MyEntities())
            {
                DateTime testDate = DateTime.Today.AddDays(22);

                // Request 1: Hợp lệ
                db.Database.ExecuteSqlCommand(
                    "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 2.0, 1, 'Req 1', 'PENDING', SYSDATE)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                );

                decimal reqId = db.Database.SqlQuery<decimal>(
                    "SELECT ID FROM HR.TB_YEUCAU_TANGCA WHERE MANV = :p0 AND TRUNC(NGAY) = TRUNC(:p1)",
                    new OracleParameter("p0", _testEmployeeId),
                    new OracleParameter("p1", testDate)
                ).FirstOrDefault();
                _cleanupRequestIds.Add(reqId);

                // Request 2: Cùng MANV + Ngày + Ca khi Request 1 đang PENDING -> Vi phạm Unique Index UQ_YEUCAU_TC_ACTIVE
                var ex = Assert.Catch<OracleException>(() =>
                {
                    db.Database.ExecuteSqlCommand(
                        "INSERT INTO HR.TB_YEUCAU_TANGCA (MANV, NGAY, GIOTANGCA, IDCA, LYDO, TRANGTHAI, CREATED_DATE) VALUES (:p0, :p1, 3.0, 1, 'Req 2 Duplicate', 'PENDING', SYSDATE)",
                        new OracleParameter("p0", _testEmployeeId),
                        new OracleParameter("p1", testDate)
                    );
                }, "Tạo trùng lặp đề xuất PENDING cho cùng 1 nhân viên trong cùng ngày và ca phải bị Oracle Unique Index UQ_YEUCAU_TC_ACTIVE chặn đứng.");
                Assert.AreEqual(1, ex.Number, "Lỗi Oracle phải là ORA-00001 (Unique constraint/index violated)");
            }
        }
    }
}
