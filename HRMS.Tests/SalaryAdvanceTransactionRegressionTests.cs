using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using NUnit.Framework;
using Oracle.ManagedDataAccess.Client;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class SalaryAdvanceTransactionRegressionTests
    {
        [TestCase(0, 11, 2026, "valid")]
        [TestCase(-1, 11, 2026, "valid")]
        [TestCase(3000000, 0, 2026, "valid")]
        [TestCase(3000000, 13, 2026, "valid")]
        [TestCase(3000000, 11, 1999, "valid")]
        [TestCase(3000000, 11, 2101, "valid")]
        [TestCase(3000000, 11, 2026, " ")]
        public void InvalidInputDoesNotReachDatabase(int amount, int month, int year, string reason)
        {
            var service = new SalaryAdvanceService("invalid connection string; must not be opened");
            Assert.Throws<ArgumentException>(() => service.CreateRequest(1, amount, new DateTime(2026,11,17), month, year, reason, 1));
        }

        // Opt-in on an authorized database. All inserted business/notification rows roll back.
        // Oracle sequence values are not transactional and can advance during this test.
        [Test, Explicit("Requires HRMS_APPROVAL_TEST_CONNECTION and an authorized Oracle fixture."), Category("OracleApprovalLive")]
        public void FailureAfterNotificationInsertRollsBackRequestAndNotification()
        {
            string cs = Environment.GetEnvironmentVariable("HRMS_APPROVAL_TEST_CONNECTION");
            if (string.IsNullOrWhiteSpace(cs)) Assert.Ignore("Oracle fixture connection not supplied.");
            const string marker = "SALARY_ADVANCE_ROLLBACK_REGRESSION";
            long requestId = 0, notificationId = 0, employee, actor;
            using (var conn = new OracleConnection(cs))
            {
                conn.Open();
                employee = Convert.ToInt64(Scalar(conn,null,"SELECT MIN(MANV) FROM HR.TB_NHANVIEN"));
                actor = Convert.ToInt64(Scalar(conn,null,"SELECT MIN(IDUSER) FROM HR.TB_SYS_USER WHERE DISABLED=0"));
            }
            Action<OracleConnection, OracleTransaction, SalaryAdvanceRequestDto> failure = (conn, tx, dto) =>
            {
                requestId = dto.Id;
                Assert.That(Scalar(conn,tx,"SELECT COUNT(*) FROM HR.TB_YEUCAU_UNGLUONG WHERE ID="+dto.Id), Is.EqualTo(1m));
                notificationId = NotificationService.CreateNotification(
                    conn,tx,marker,marker,"VERIFICATION","SYSTEM_VERIFIER",
                    targetType:"PERSONAL",targetManv:employee,eventType:marker,entityId:dto.Id);
                Assert.That(Scalar(conn,tx,"SELECT COUNT(*) FROM HR.TB_THONGBAO WHERE ID="+notificationId), Is.EqualTo(1m));
                throw new InvalidOperationException(marker);
            };
            var constructor = typeof(SalaryAdvanceService).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,null,
                new[] { typeof(string),typeof(Action<OracleConnection,OracleTransaction,SalaryAdvanceRequestDto>) },null);
            Assert.That(constructor, Is.Not.Null);
            var service = (SalaryAdvanceService)constructor.Invoke(new object[] {cs,failure});
            var error = Assert.Throws<InvalidOperationException>(() =>
                service.CreateRequest(employee,3000000,new DateTime(2026,11,17),11,2026,marker,actor));
            Assert.That(error.Message,Is.EqualTo(marker));
            Assert.That(requestId,Is.GreaterThan(0));
            Assert.That(notificationId,Is.GreaterThan(0));
            using (var conn=new OracleConnection(cs))
            {
                conn.Open();
                Assert.That(Scalar(conn,null,"SELECT COUNT(*) FROM HR.TB_YEUCAU_UNGLUONG WHERE ID="+requestId),Is.Zero);
                Assert.That(Scalar(conn,null,"SELECT COUNT(*) FROM HR.TB_THONGBAO WHERE ID="+notificationId),Is.Zero);
                Assert.That(Scalar(conn,null,"SELECT COUNT(*) FROM HR.TB_PUSH_OUTBOX WHERE NOTIFICATION_ID="+notificationId),Is.Zero);
            }
        }
        private static decimal Scalar(OracleConnection conn, OracleTransaction tx, string sql)
        {
            using (var cmd=new OracleCommand(sql,conn))
            {
                cmd.Transaction=tx;
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }
    }
}

