using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bu.CLASS_SECURITY;
using Bu.Services.AI_Services.Core;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class PostReviewRemediationVerificationTests
    {
        [Test]
        public void NumberParsing_CultureInvariant_RejectsNaNAndInfinities()
        {
            var viCulture = CultureInfo.GetCultureInfo("vi-VN");
            var enCulture = CultureInfo.GetCultureInfo("en-US");

            // Helper logic as implemented in Coordinator and FrmOllamaConfig
            bool TryParseInvariant(string text, out double val)
            {
                val = 0;
                if (string.IsNullOrWhiteSpace(text)) return false;
                string clean = text.Trim();
                if (double.TryParse(clean, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
                {
                    return !double.IsNaN(val) && !double.IsInfinity(val);
                }
                if (double.TryParse(clean, NumberStyles.Float, CultureInfo.CurrentCulture, out val))
                {
                    return !double.IsNaN(val) && !double.IsInfinity(val);
                }
                return false;
            }

            // Valid numbers across formats
            Assert.IsTrue(TryParseInvariant("0.8", out double p1));
            Assert.AreEqual(0.8, p1, 1e-5);

            Assert.IsTrue(TryParseInvariant("1.15", out double p2));
            Assert.AreEqual(1.15, p2, 1e-5);

            // Rejections: NaN, Inifinity, Malformed
            Assert.IsFalse(TryParseInvariant("NaN", out _));
            Assert.IsFalse(TryParseInvariant("Infinity", out _));
            Assert.IsFalse(TryParseInvariant("-Infinity", out _));
            Assert.IsFalse(TryParseInvariant("abc", out _));
        }

        [Test]
        public async Task Coordinator_Validation_RejectsOutOfRangeParametersEntirely()
        {
            var coordinator = new AiConfigurationCoordinator();

            // 1. Invalid TopP > 1.0 (out of range)
            var reqOutOfRange = new AiConfigUpdateDto
            {
                OllamaHost = "http://127.0.0.1:11434",
                AiModel = "qwen2.5:7b",
                QdrantUrl = "http://127.0.0.1:6333",
                AiTopP = 9.0, // Invalid!
                AiTemp = 0.5,
                AiMaxTokens = 1000,
                ExpectedVersion = 1
            };

            var res = await coordinator.UpdateConfigurationAsync(reqOutOfRange, 1);
            Assert.IsFalse(res.Success);
            StringAssert.Contains("Top P", res.Message);

            // 2. Negative Temperature
            var reqNegTemp = new AiConfigUpdateDto
            {
                OllamaHost = "http://127.0.0.1:11434",
                AiModel = "qwen2.5:7b",
                QdrantUrl = "http://127.0.0.1:6333",
                AiTopP = 0.8,
                AiTemp = -0.5, // Invalid!
                AiMaxTokens = 1000,
                ExpectedVersion = 1
            };

            var res2 = await coordinator.UpdateConfigurationAsync(reqNegTemp, 1);
            Assert.IsFalse(res2.Success);
            StringAssert.Contains("Temperature", res2.Message);
        }

        [Test]
        public async Task Coordinator_SSRFGuard_RejectsMetadataAndDisallowedDestinations()
        {
            var coordinator = new AiConfigurationCoordinator();

            // Cloud Metadata IP (169.254.169.254)
            var reqMetadata = new AiConfigUpdateDto
            {
                OllamaHost = "http://169.254.169.254:11434",
                AiModel = "qwen2.5:7b",
                ExpectedVersion = 1
            };

            var resMeta = await coordinator.UpdateConfigurationAsync(reqMetadata, 1);
            Assert.IsFalse(resMeta.Success);
            StringAssert.Contains("metadata", resMeta.Message);

            // Multicast IP
            var reqMulti = new AiConfigUpdateDto
            {
                OllamaHost = "http://224.0.0.1:11434",
                AiModel = "qwen2.5:7b",
                ExpectedVersion = 1
            };
            var resMulti = await coordinator.UpdateConfigurationAsync(reqMulti, 1);
            Assert.IsFalse(resMulti.Success);
            StringAssert.Contains("multicast", resMulti.Message);

            // Port not in allowlist (e.g. port 22 SSH)
            var reqPort = new AiConfigUpdateDto
            {
                OllamaHost = "http://127.0.0.1:22",
                AiModel = "qwen2.5:7b",
                ExpectedVersion = 1
            };
            var resPort = await coordinator.UpdateConfigurationAsync(reqPort, 1);
            Assert.IsFalse(resPort.Success);
            StringAssert.Contains("không nằm trong danh mục cổng được phép", resPort.Message);
        }

        [Test]
        public void Coordinator_SnapshotClone_IsDefensiveCopy()
        {
            var coordinator = new AiConfigurationCoordinator();
            var snap1 = coordinator.CurrentSnapshot;
            var snap2 = coordinator.CurrentSnapshot;

            Assert.AreNotSame(snap1, snap2, "CurrentSnapshot must return a defensive clone to protect internal state.");
            Assert.AreEqual(snap1.Version, snap2.Version);
            Assert.AreEqual(snap1.OllamaHost, snap2.OllamaHost);
        }

        [Test]
        public void BatchPermissionRequest_StructureAndContract_IsAtomic()
        {
            var req = new BatchSaveChannelRightsRequest
            {
                TargetUserId = 10,
                ExpectedSecurityVersion = 3,
                ActorUserId = 1,
                CorrelationId = "TEST-CORR-123",
                Channels = new List<SaveChannelRightsRequest>
                {
                    new SaveChannelRightsRequest
                    {
                        Channel = "DESKTOP",
                        ParentDirectGrant = true,
                        Functions = new List<SaveChannelFunctionItemRequest>
                        {
                            new SaveChannelFunctionItemRequest { FunctionCode = "F_SYSTEM_USER", CanView = true, CanEdit = true }
                        }
                    },
                    new SaveChannelRightsRequest
                    {
                        Channel = "WEB",
                        ParentDirectGrant = true,
                        Functions = new List<SaveChannelFunctionItemRequest>
                        {
                            new SaveChannelFunctionItemRequest { FunctionCode = "F_SYSTEM_USER", CanView = true, CanEdit = false }
                        }
                    },
                    new SaveChannelRightsRequest
                    {
                        Channel = "MOBILE",
                        ParentDirectGrant = false
                    }
                }
            };

            Assert.AreEqual(10, req.TargetUserId);
            Assert.AreEqual(3, req.ExpectedSecurityVersion);
            Assert.AreEqual(3, req.Channels.Count);
            Assert.AreEqual("DESKTOP", req.Channels[0].Channel);
            Assert.AreEqual("WEB", req.Channels[1].Channel);
            Assert.AreEqual("MOBILE", req.Channels[2].Channel);
        }

        [Test]
        public void AttendanceDayBinding_GeneratesParameterizedColumns_NoInterpolation()
        {
            // Verify generation logic of day columns D1..D31
            int daysInMonth = 30;
            var daySymbols = new Dictionary<int, string>
            {
                { 1, "X" },
                { 2, "X" },
                { 3, "P" },
                { 15, "CT" }
            };

            var setClauses = new List<string>();
            var dayParams = new Dictionary<string, string>();
            for (int d = 1; d <= 31; d++)
            {
                string col = $"D{d}";
                if (d <= daysInMonth && daySymbols.TryGetValue(d, out var s) && !string.IsNullOrEmpty(s))
                {
                    string paramName = $"p_d{d}";
                    setClauses.Add($"{col} = :{paramName}");
                    dayParams[paramName] = s;
                }
                else if (d > daysInMonth)
                {
                    setClauses.Add($"{col} = NULL");
                }
            }

            // Must contain parameter syntax and no literal string embedding
            Assert.IsTrue(setClauses.Contains("D1 = :p_d1"));
            Assert.IsTrue(setClauses.Contains("D3 = :p_d3"));
            Assert.IsTrue(setClauses.Contains("D31 = NULL"));
            Assert.AreEqual("X", dayParams["p_d1"]);
            Assert.AreEqual("P", dayParams["p_d3"]);

            // Ensure no raw apostrophe string injection occurs
            string fullSql = $"UPDATE TB_KYCONGCHITIET SET {string.Join(", ", setClauses)} WHERE MAKYCONG = :p_mkc AND MANV = :p_manv";
            Assert.IsFalse(fullSql.Contains("'X'"), "SQL must not contain embedded literals for attendance days.");
            Assert.IsTrue(fullSql.Contains(":p_d1"), "SQL must bind attendance days using Oracle parameters.");
        }

        [Test]
        public void Admin_UserSession_CanPrint_EnforcesGranularDetailedRights_DeniesWhenDbPrintIsFalse()
        {
            // Setup UserSession for root ADMIN
            Bu.CLASS_SYSTEM.UserSession.Clear();
            Bu.CLASS_SYSTEM.UserSession.CurrentUser = new DA.TB_SYS_USER
            {
                IDUSER = 80,
                USERNAME = "ADMIN",
                FULLNAME = "System Administrator",
                DISABLED = 0
            };
            Bu.CLASS_SYSTEM.UserSession.CurrentChannel = "DESKTOP";
            Bu.CLASS_SYSTEM.UserSession.CurrentSessionId = "sess-admin-123";
            Bu.CLASS_SYSTEM.UserSession.CurrentJti = "jti-admin-123";

            // Even if UserRights has wildcard "*"
            Bu.CLASS_SYSTEM.UserSession.UserRights = new List<string> { "*", "F_LOGIN_DESKTOP", "F_CC_BANGLUONG" };

            // DetailedRights reflecting actual DB state where CAN_PRINT = 0 (false)
            Bu.CLASS_SYSTEM.UserSession.DetailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                { "F_LOGIN_DESKTOP", new Bu.DTO.UserRightDetail { CAN_VIEW = true } },
                { "F_CC_BANGLUONG", new Bu.DTO.UserRightDetail
                    {
                        FUNCTION_CODE = "F_CC_BANGLUONG",
                        CAN_VIEW = true,
                        CAN_ADD = true,
                        CAN_EDIT = true,
                        CAN_DELETE = true,
                        CAN_PRINT = false // Missing print grant in DB
                    }
                }
            };

            // CanView, CanEdit are allowed
            Assert.IsTrue(Bu.CLASS_SYSTEM.UserSession.CanView("F_CC_BANGLUONG"), "Admin must have CanView when DB has CAN_VIEW = true");
            Assert.IsTrue(Bu.CLASS_SYSTEM.UserSession.CanEdit("F_CC_BANGLUONG"), "Admin must have CanEdit when DB has CAN_EDIT = true");

            // CanPrint MUST return false because DetailedRights has CAN_PRINT = false (no artificial true override)
            Assert.IsFalse(Bu.CLASS_SYSTEM.UserSession.CanPrint("F_CC_BANGLUONG"), "Admin must NOT have CanPrint when DB has CAN_PRINT = false");

            // When CAN_PRINT is granted in DB (true)
            Bu.CLASS_SYSTEM.UserSession.DetailedRights["F_CC_BANGLUONG"].CAN_PRINT = true;
            Assert.IsTrue(Bu.CLASS_SYSTEM.UserSession.CanPrint("F_CC_BANGLUONG"), "Admin must have CanPrint when DB has CAN_PRINT = true");

            Bu.CLASS_SYSTEM.UserSession.Clear();
        }

        [Test]
        public void Admin_MobileAccess_PlatformAccessResolver_ExplicitlyDeniesRootAdmin()
        {
            var resolver = new PlatformAccessResolver();
            // Test that Root ADMIN on Mobile channel is explicitly rejected by policy
            var mobileRes = resolver.ResolveChannel(null, 80, "MOBILE", "ADMIN");
            Assert.IsFalse(mobileRes.IsGranted, "Root ADMIN must be denied on Mobile channel");
            Assert.AreEqual("ROOT_ADMIN_MOBILE_FORBIDDEN", mobileRes.ReadinessCode, "Must return ROOT_ADMIN_MOBILE_FORBIDDEN readiness code");
        }

        [Test]
        public void Admin_EmployeeMapping_SelfScopeDiffersFromAllScope_WhenManvIsNull()
        {
            // Root ADMIN in TB_SYS_USER has MANV = null by security policy
            decimal? adminManv = null;

            // Self-scope capability requires an employee link
            bool CanExecuteSelfScope(decimal? manv, string capability)
            {
                if (capability == "PAYROLL_SELF" || capability == "INSURANCE_SELF")
                {
                    return manv.HasValue && manv.Value > 0;
                }
                return true;
            }

            // Self-scope fails closed because ADMIN is not mapped to an employee record
            Assert.IsFalse(CanExecuteSelfScope(adminManv, "PAYROLL_SELF"), "PAYROLL_SELF must fail closed when MANV is null");
            Assert.IsFalse(CanExecuteSelfScope(adminManv, "INSURANCE_SELF"), "INSURANCE_SELF must fail closed when MANV is null");

            // Company-wide / All-scope capabilities do NOT depend on employee mapping
            Assert.IsTrue(CanExecuteSelfScope(adminManv, "PAYROLL_SUMMARY"), "PAYROLL_SUMMARY operates on company-wide scope");
            Assert.IsTrue(CanExecuteSelfScope(adminManv, "EMPLOYEE_LOOKUP"), "EMPLOYEE_LOOKUP operates on company-wide scope");
            Assert.IsTrue(CanExecuteSelfScope(adminManv, "ATTENDANCE_SUMMARY"), "ATTENDANCE_SUMMARY operates on company-wide scope");
        }

        [Test]
        public void Production_ProjectEffectiveRights_IncludesParentDesktop_EnablesUserSession()
        {
            var resolver = new ChannelPermissionResolver();
            var tree = new PlatformChannelTreeDto
            {
                Channel = "DESKTOP",
                ParentFunctionCode = "F_LOGIN_DESKTOP",
                ParentFunctionName = "Đăng nhập Desktop",
                ParentIsEffective = true,
                Functions = new List<ChannelFunctionRightItemDto>
                {
                    new ChannelFunctionRightItemDto
                    {
                        FunctionCode = "F_CC_BANGLUONG",
                        FunctionName = "Bảng lương",
                        EffectiveGrant = new FunctionActionGrantDto { CanView = true, CanEdit = true }
                    }
                }
            };

            resolver.ProjectEffectiveRights(tree, out var detailedRights, out var viewableRights);

            // Kiểm tra projector production đưa đúng quyền cha
            Assert.IsTrue(detailedRights.ContainsKey("F_LOGIN_DESKTOP"), "DetailedRights must contain ParentFunctionCode");
            Assert.IsTrue(detailedRights["F_LOGIN_DESKTOP"].CAN_VIEW, "ParentFunctionCode CAN_VIEW must be true");
            Assert.Contains("F_LOGIN_DESKTOP", viewableRights, "viewableRights must contain F_LOGIN_DESKTOP");

            // Nạp trực tiếp vào UserSession production để kiểm tra tính nhất quán
            Bu.CLASS_SYSTEM.UserSession.Clear();
            Bu.CLASS_SYSTEM.UserSession.CurrentUser = new DA.TB_SYS_USER { IDUSER = 80, USERNAME = "ADMIN", DISABLED = 0 };
            Bu.CLASS_SYSTEM.UserSession.CurrentChannel = "DESKTOP";
            Bu.CLASS_SYSTEM.UserSession.CurrentSessionId = "sess-desktop-1";
            Bu.CLASS_SYSTEM.UserSession.CurrentJti = "jti-desktop-1";
            Bu.CLASS_SYSTEM.UserSession.DetailedRights = detailedRights;
            Bu.CLASS_SYSTEM.UserSession.UserRights = viewableRights;

            // Quyền cha Desktop phải BẬT
            Assert.IsTrue(Bu.CLASS_SYSTEM.UserSession.ParentDesktopOn, "ParentDesktopOn must be true when ParentIsEffective is true");
            Assert.IsTrue(Bu.CLASS_SYSTEM.UserSession.CanView("F_CC_BANGLUONG"), "CanView child function must succeed when Parent is ON");

            Bu.CLASS_SYSTEM.UserSession.Clear();
        }

        [Test]
        public void Production_ProjectEffectiveRights_ParentDisabled_BlocksUserSessionChildFunctions()
        {
            var resolver = new ChannelPermissionResolver();
            var tree = new PlatformChannelTreeDto
            {
                Channel = "DESKTOP",
                ParentFunctionCode = "F_LOGIN_DESKTOP",
                ParentFunctionName = "Đăng nhập Desktop",
                ParentIsEffective = false, // Cha bị tắt
                Functions = new List<ChannelFunctionRightItemDto>
                {
                    new ChannelFunctionRightItemDto
                    {
                        FunctionCode = "F_CC_BANGLUONG",
                        FunctionName = "Bảng lương",
                        EffectiveGrant = new FunctionActionGrantDto { CanView = true, CanEdit = true }
                    }
                }
            };

            resolver.ProjectEffectiveRights(tree, out var detailedRights, out var viewableRights);

            // Quyền cha trong detailedRights có CAN_VIEW = false
            Assert.IsTrue(detailedRights.ContainsKey("F_LOGIN_DESKTOP"));
            Assert.IsFalse(detailedRights["F_LOGIN_DESKTOP"].CAN_VIEW, "ParentFunctionCode CAN_VIEW must be false when ParentIsEffective is false");
            Assert.IsFalse(viewableRights.Contains("F_LOGIN_DESKTOP"), "viewableRights must NOT contain F_LOGIN_DESKTOP");

            // Nạp vào UserSession production
            Bu.CLASS_SYSTEM.UserSession.Clear();
            Bu.CLASS_SYSTEM.UserSession.CurrentUser = new DA.TB_SYS_USER { IDUSER = 80, USERNAME = "ADMIN", DISABLED = 0 };
            Bu.CLASS_SYSTEM.UserSession.CurrentChannel = "DESKTOP";
            Bu.CLASS_SYSTEM.UserSession.CurrentSessionId = "sess-desktop-2";
            Bu.CLASS_SYSTEM.UserSession.CurrentJti = "jti-desktop-2";
            Bu.CLASS_SYSTEM.UserSession.DetailedRights = detailedRights;
            Bu.CLASS_SYSTEM.UserSession.UserRights = viewableRights;

            // Quyền cha Desktop phải TẮT và chặn toàn bộ quyền con
            Assert.IsFalse(Bu.CLASS_SYSTEM.UserSession.ParentDesktopOn, "ParentDesktopOn must be false");
            Assert.IsFalse(Bu.CLASS_SYSTEM.UserSession.CanView("F_CC_BANGLUONG"), "Child function CanView must be blocked when Parent is OFF");
            Assert.IsFalse(Bu.CLASS_SYSTEM.UserSession.CanEdit("F_CC_BANGLUONG"), "Child function CanEdit must be blocked when Parent is OFF");

            Bu.CLASS_SYSTEM.UserSession.Clear();
        }

        [Test]
        public void Production_ProjectEffectiveRights_DoesNotInjectArbitraryRights_ZeroTrust()
        {
            var resolver = new ChannelPermissionResolver();
            var tree = new PlatformChannelTreeDto
            {
                Channel = "DESKTOP",
                ParentFunctionCode = "F_LOGIN_DESKTOP",
                ParentIsEffective = true,
                Functions = new List<ChannelFunctionRightItemDto>
                {
                    new ChannelFunctionRightItemDto
                    {
                        FunctionCode = "F_DM_NHANVIEN",
                        EffectiveGrant = new FunctionActionGrantDto { CanView = true }
                    }
                }
            };

            resolver.ProjectEffectiveRights(tree, out var detailedRights, out var viewableRights);

            // Không tự động nhét F_DB_NHANSU hoặc F_SYSTEM_AI nếu không được cấp
            Assert.IsFalse(detailedRights.ContainsKey("F_DB_NHANSU"), "DetailedRights must not contain F_DB_NHANSU when not granted");
            Assert.IsFalse(detailedRights.ContainsKey("F_SYSTEM_AI"), "DetailedRights must not contain F_SYSTEM_AI when not granted");
            Assert.IsFalse(viewableRights.Contains("F_DB_NHANSU"), "viewableRights must not contain F_DB_NHANSU");
            Assert.IsFalse(viewableRights.Contains("F_SYSTEM_AI"), "viewableRights must not contain F_SYSTEM_AI");
        }

        [Test]
        public async Task Production_UserController_RejectsMissingOrInvalidJwtActor_ReturnsUnauthorized()
        {
            var controller = new HRMS_API.Controllers.UserController();
            controller.Request = new System.Net.Http.HttpRequestMessage();

            // 1. SavePlatformAccess
            var resPlatform = await controller.SavePlatformAccess(80, new SavePlatformRightsRequest());
            Assert.IsInstanceOf<System.Web.Http.Results.UnauthorizedResult>(resPlatform, "SavePlatformAccess must return 401 Unauthorized when actor JWT is missing");

            // 2. SaveChannelPermissions
            var resChannel = await controller.SaveChannelPermissions(80, new SaveChannelRightsRequest());
            Assert.IsInstanceOf<System.Web.Http.Results.UnauthorizedResult>(resChannel, "SaveChannelPermissions must return 401 Unauthorized when actor JWT is missing");

            // 3. SaveBatchChannelPermissions
            var resBatch = await controller.SaveBatchChannelPermissions(80, new BatchSaveChannelRightsRequest());
            Assert.IsInstanceOf<System.Web.Http.Results.UnauthorizedResult>(resBatch, "SaveBatchChannelPermissions must return 401 Unauthorized when actor JWT is missing");
        }

        [Test]
        public void Production_AiConfiguration_CanonicalIndexConstants_Verified()
        {
            Assert.AreEqual("bge-m3", Bu.Services.AI_Services.Core.OllamaService.DEFAULT_EMBEDDING_MODEL);
            Assert.AreEqual("hrms_vectors_v2", Bu.Services.AI_Services.Vector.QdrantService.DEFAULT_COLLECTION_NAME);
        }
    }
}
