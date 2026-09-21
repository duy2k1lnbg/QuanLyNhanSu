using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bu.Tests
{
    [TestFixture]
    public class PermissionFiveActionsTests
    {
        [TearDown]
        public void Cleanup()
        {
            UserSession.CurrentUser = null;
            UserSession.DetailedRights = null;
        }

        [Test]
        public void UserRightDetail_Properties_SetAndGetCorrectly()
        {
            var detail = new UserRightDetail
            {
                FunctionCode = "F_DM_NHANVIEN",
                FunctionName = "Nhân Viên",
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = false,
                CanPrint = true
            };

            Assert.AreEqual("F_DM_NHANVIEN", detail.FunctionCode);
            Assert.AreEqual("Nhân Viên", detail.FunctionName);
            Assert.IsTrue(detail.CanView);
            Assert.IsTrue(detail.CanAdd);
            Assert.IsTrue(detail.CanEdit);
            Assert.IsFalse(detail.CanDelete);
            Assert.IsTrue(detail.CanPrint);
        }

        [Test]
        public void UserSession_AdminUser_HasAllPermissions_EvenWithoutExplicitRights()
        {
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 1,
                USERNAME = "admin",
                FULLNAME = "Administrator"
            };
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>();

            Assert.IsTrue(UserSession.IsAdmin);
            Assert.IsTrue(UserSession.CanView("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanAdd("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanEdit("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanDelete("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanPrint("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.HasRight("F_DM_NHANVIEN"));

            Assert.IsTrue(UserSession.CheckPermission("ANY_UNKNOWN_FUNCTION", PermissionAction.Delete));
        }

        [Test]
        public void UserSession_RegularUser_EnforcesFiveGranularPermissions()
        {
            UserSession.CurrentUser = new TB_SYS_USER
            {
                IDUSER = 999,
                USERNAME = "staff_user",
                FULLNAME = "Nhân viên thử nghiệm"
            };

            var rights = new Dictionary<string, UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                ["F_DM_NHANVIEN"] = new UserRightDetail
                {
                    FunctionCode = "F_DM_NHANVIEN",
                    CanView = true,
                    CanAdd = true,
                    CanEdit = true,
                    CanDelete = false,
                    CanPrint = true
                },
                ["F_DM_PHONGBAN"] = new UserRightDetail
                {
                    FunctionCode = "F_DM_PHONGBAN",
                    CanView = true,
                    CanAdd = false,
                    CanEdit = false,
                    CanDelete = false,
                    CanPrint = false
                }
            };
            UserSession.DetailedRights = rights;

            Assert.IsFalse(UserSession.IsAdmin);

            // F_DM_NHANVIEN: View=1, Add=1, Edit=1, Delete=0, Print=1
            Assert.IsTrue(UserSession.CanView("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanAdd("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanEdit("F_DM_NHANVIEN"));
            Assert.IsFalse(UserSession.CanDelete("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.CanPrint("F_DM_NHANVIEN"));
            Assert.IsTrue(UserSession.HasRight("F_DM_NHANVIEN"));

            // CheckPermission using Enum
            Assert.IsTrue(UserSession.CheckPermission("F_DM_NHANVIEN", PermissionAction.View));
            Assert.IsTrue(UserSession.CheckPermission("F_DM_NHANVIEN", PermissionAction.Add));
            Assert.IsTrue(UserSession.CheckPermission("F_DM_NHANVIEN", PermissionAction.Edit));
            Assert.IsFalse(UserSession.CheckPermission("F_DM_NHANVIEN", PermissionAction.Delete));
            Assert.IsTrue(UserSession.CheckPermission("F_DM_NHANVIEN", PermissionAction.Print));

            // F_DM_PHONGBAN: View-only
            Assert.IsTrue(UserSession.CanView("F_DM_PHONGBAN"));
            Assert.IsFalse(UserSession.CanAdd("F_DM_PHONGBAN"));
            Assert.IsFalse(UserSession.CanEdit("F_DM_PHONGBAN"));
            Assert.IsFalse(UserSession.CanDelete("F_DM_PHONGBAN"));
            Assert.IsFalse(UserSession.CanPrint("F_DM_PHONGBAN"));

            // Unassigned function: All false
            Assert.IsFalse(UserSession.CanView("F_NV_HOPDONG"));
            Assert.IsFalse(UserSession.CanAdd("F_NV_HOPDONG"));
            Assert.IsFalse(UserSession.CanEdit("F_NV_HOPDONG"));
            Assert.IsFalse(UserSession.CanDelete("F_NV_HOPDONG"));
            Assert.IsFalse(UserSession.CanPrint("F_NV_HOPDONG"));
        }

        [Test]
        public void SYS_USER_GetDetailedRights_QueriesOracleDb_ReturnsGranularRights()
        {
            var sysUser = new SYS_USER();
            // Lấy danh sách quyền của user 1 (hoặc user đầu tiên trong hệ thống)
            var users = sysUser.getALL();
            Assert.IsNotNull(users);
            if (users.Count > 0)
            {
                var firstUser = users[0];
                var detailedRights = sysUser.GetDetailedRights(firstUser.IDUSER);

                Assert.IsNotNull(detailedRights);
                Assert.That(detailedRights.Count, Is.GreaterThan(0));

                foreach (var kvp in detailedRights)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(kvp.Key));
                    Assert.IsNotNull(kvp.Value);
                    Assert.AreEqual(kvp.Key, kvp.Value.FunctionCode);
                }
            }
        }

        [Test]
        public void SYS_USER_GetRights_BackwardCompatibility_MatchesCanView()
        {
            var sysUser = new SYS_USER();
            var users = sysUser.getALL();
            if (users.Count > 0)
            {
                var firstUser = users[0];
                var oldRights = sysUser.GetRights(firstUser.IDUSER);
                var detailedRights = sysUser.GetDetailedRights(firstUser.IDUSER);

                Assert.IsNotNull(oldRights);
                Assert.IsNotNull(detailedRights);

                // Mọi quyền có CanView = true phải có trong GetRights
                var viewableCodes = detailedRights.Values
                    .Where(r => r.CanView)
                    .Select(r => r.FunctionCode)
                    .ToList();

                foreach (var code in viewableCodes)
                {
                    Assert.IsTrue(oldRights.Contains(code), $"Mã chức năng {code} có CanView=true nhưng thiếu trong GetRights.");
                }
            }
        }

        [Test]
        public void SYS_USER_EnsureSeeded_SeedsAllNewFunctions()
        {
            var sysUser = new SYS_USER();
            sysUser.EnsureSeeded();

            using (var db = new MyEntities())
            {
                var allDbFuncs = db.TB_SYS_FUNCTION.Select(x => x.FUNCTION_CODE).ToList();
                Console.WriteLine($"Total functions in DB now: {allDbFuncs.Count}");

                string[] expected = new string[] {
                    "F_NV_PHEDUYET", "F_SYSTEM_THONGBAO", "F_SYSTEM_CAPTAIKHOAN", 
                    "F_SYSTEM_PQ_CHUCNANG", "F_SYSTEM_PQ_BAOCAO", "F_SYSTEM_DB_CONFIG", 
                    "F_CC_BCCT_IN", "F_CC_CAPNHATCONG", "MOBILE_REQUEST_LEAVE",
                    "MOBILE_REQUEST_OVERTIME", "MOBILE_REQUEST_ADVANCE"
                };

                foreach (var exp in expected)
                {
                    Assert.IsTrue(allDbFuncs.Contains(exp), $"Function {exp} is missing from Oracle database TB_SYS_FUNCTION table.");
                }
            }
        }
    }
}

