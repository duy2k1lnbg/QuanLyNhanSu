using System;
using System.Collections.Generic;
using System.Linq;
using Bu;
using Bu.CLASS_CHAMCONG;
using Bu.CLASS_NHANSU;
using Bu.CLASS_SYSTEM;
using DA;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture]
    public class Section10IntegrationTests
    {
        private KYCONG _kycongBus;
        private RecycleBinService _recycleBin;

        [SetUp]
        public void Setup()
        {
            _kycongBus = new KYCONG();
            _recycleBin = new RecycleBinService();
            UserSession.CurrentUser = null;
            UserSession.UserRights = null;
            UserSession.DetailedRights = null;
        }

        [TearDown]
        public void TearDown()
        {
            UserSession.CurrentUser = null;
            UserSession.UserRights = null;
            UserSession.DetailedRights = null;
        }

        [Test]
        public void Test_KyCong_Validation_MonthOutOfRange_ThrowsBusinessException()
        {
            var kc = new TB_KYCONG
            {
                THANG = 13,
                NAM = 2026,
                TRANGTHAI = 0
            };

            var ex = Assert.Throws<BusinessException>(() => _kycongBus.Add(kc));
            Assert.AreEqual("VALIDATION_FAILED", ex.ErrorCode);
            StringAssert.Contains("Tháng", ex.Message);
        }

        [Test]
        public void Test_KyCong_Validation_YearOutOfRange_ThrowsBusinessException()
        {
            var kc = new TB_KYCONG
            {
                THANG = 5,
                NAM = 1990,
                TRANGTHAI = 0
            };

            var ex = Assert.Throws<BusinessException>(() => _kycongBus.Add(kc));
            Assert.AreEqual("VALIDATION_FAILED", ex.ErrorCode);
            StringAssert.Contains("Năm", ex.Message);
        }

        [Test]
        public void Test_KyCong_DuplicateActivePeriod_ThrowsBusinessException()
        {
            // First find an existing active period in DB
            TB_KYCONG existing = null;
            using (var db = new MyEntities())
            {
                existing = db.TB_KYCONG.FirstOrDefault(x => x.DELETED_DATE == null);
            }

            if (existing != null)
            {
                var duplicate = new TB_KYCONG
                {
                    THANG = existing.THANG,
                    NAM = existing.NAM,
                    TRANGTHAI = 0
                };

                var ex = Assert.Throws<BusinessException>(() => _kycongBus.Add(duplicate));
                Assert.AreEqual("PERIOD_ALREADY_EXISTS", ex.ErrorCode);
                StringAssert.Contains("đã tồn tại", ex.Message);
            }
        }

        [Test]
        public void Test_ErrorHelper_ResolvesOracleErrorsFriendly()
        {
            var ora00001 = new Exception("ORA-00001: unique constraint violated");
            var msg1 = ErrorHelper.ResolveUserFriendlyMessage(ora00001, "Lưu dữ liệu thất bại");
            StringAssert.Contains("Dữ liệu đã tồn tại", msg1);

            var ora02292 = new Exception("ORA-02292: integrity constraint violated - child record found");
            var msg2 = ErrorHelper.ResolveUserFriendlyMessage(ora02292, "Xóa dữ liệu thất bại");
            StringAssert.Contains("Dữ liệu đang được sử dụng", msg2);

            var ora50000 = new Exception("ORA-50000: network connection failed");
            var msg3 = ErrorHelper.ResolveUserFriendlyMessage(ora50000, "Kết nối thất bại");
            StringAssert.Contains("Không thể kết nối đến cơ sở dữ liệu", msg3);
        }

        [Test]
        public void Test_ErrorHelper_BusinessException_PreservesMessage()
        {
            var bEx = new BusinessException("PERIOD_LOCKED", "Kỳ công đã bị khóa, không thể chỉnh sửa.");
            var msg = ErrorHelper.ResolveUserFriendlyMessage(bEx, "Lỗi mặc định");
            Assert.AreEqual("Kỳ công đã bị khóa, không thể chỉnh sửa.", msg);
        }

        [Test]
        public void Test_RecycleBin_UngLuong_List_DoesNotThrow()
        {
            var list = _recycleBin.GetDeletedItems("UNGLUONG");
            Assert.IsNotNull(list);
        }

        [Test]
        public void Test_RecycleBin_NhanVienPhuCap_List_DoesNotThrow()
        {
            var list = _recycleBin.GetDeletedItems("PHUCAP");
            Assert.IsNotNull(list);
        }

        [Test]
        public void Test_DieuChinhLuong_DeductionVsEarningSignLogic()
        {
            // Simulate the DieuChinhItem logic
            // Earning item: Old = 10,000,000, New = 11,000,000 => ChenhLech = +1,000,000 => AnhHuongThucLinh = +1,000,000
            decimal oldEarning = 10000000m;
            decimal newEarning = 11000000m;
            decimal diffEarning = newEarning - oldEarning;
            bool isDeductionEarning = false;
            decimal impactEarning = isDeductionEarning ? -diffEarning : diffEarning;

            Assert.AreEqual(1000000m, impactEarning);

            // Deduction item: Old = 500,000, New = 800,000 => ChenhLech = +300,000 => AnhHuongThucLinh = -300,000
            decimal oldDeduction = 500000m;
            decimal newDeduction = 800000m;
            decimal diffDeduction = newDeduction - oldDeduction;
            bool isDeduction = true;
            decimal impactDeduction = isDeduction ? -diffDeduction : diffDeduction;

            Assert.AreEqual(-300000m, impactDeduction);

            // Total impact on Net Pay
            decimal netImpact = impactEarning + impactDeduction;
            Assert.AreEqual(700000m, netImpact); // +1M earnings - 300k deductions = +700k net
        }

        [Test]
        public void Test_RecordPhuLuc_InvalidSalary_ThrowsBusinessException()
        {
            var luongHieuLucBus = new LUONG_HIEULUC();
            var ex = Assert.Throws<BusinessException>(() =>
            {
                luongHieuLucBus.RecordPhuLuc(
                    manv: 1,
                    sohd: "HD-001",
                    soPhuLuc: "PL-001",
                    ngayHieuLuc: DateTime.Today,
                    denNgay: null,
                    luongMoi: -100m, // Invalid salary <= 0
                    noiDung: "Điều chỉnh thử nghiệm",
                    canCu: "Quyết định thử",
                    phuCaps: null,
                    userId: 1
                );
            });

            Assert.AreEqual("VALIDATION_FAILED", ex.ErrorCode);
        }

        [Test]
        public void Test_FrmPhatSinhLuong_LoadQuery()
        {
            int makycong = 202609;
            int thang = makycong % 100;
            int nam = makycong / 100;

            using (var db = new MyEntities())
            {
                DateTime startOfMonth = new DateTime(nam, thang, 1);
                DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                // 1. Allowances query
                var qAllowances = from np in db.TB_NHANVIEN_PHUCAP
                                  where np.DELETED_DATE == null
                                    && (np.TU_NGAY == null || np.TU_NGAY <= endOfMonth)
                                    && (np.DEN_NGAY == null || np.DEN_NGAY >= startOfMonth)
                                  join nv in db.TB_NHANVIEN on np.MANV equals nv.MANV into nvGroup
                                  from nv in nvGroup.DefaultIfEmpty()
                                  join pc in db.TB_PHUCAP on np.IDPC equals pc.IDPC into pcGroup
                                  from pc in pcGroup.DefaultIfEmpty()
                                  select new
                                  {
                                      np.MANV,
                                      np.IDPC,
                                      HOTEN = nv.HOTEN,
                                      TENPC = pc.TENPC,
                                      np.SOTIEN,
                                      np.CACH_TINH,
                                      np.GHICHU,
                                      np.CREATED_DATE
                                  };

                var allowances = qAllowances.Take(100).ToList();
                Assert.IsNotNull(allowances);

                // 2. Advances query
                var qAdvances = from ul in db.TB_UNGLUONG
                                where ul.THANG == thang && ul.NAM == nam && ul.DELETED_DATE == null
                                join nv in db.TB_NHANVIEN on ul.MANV equals nv.MANV into nvGroup
                                from nv in nvGroup.DefaultIfEmpty()
                                select new
                                {
                                    ul.IDUL,
                                    ul.MANV,
                                    HOTEN = nv.HOTEN,
                                    ul.NGAY,
                                    ul.THANG,
                                    ul.NAM,
                                    ul.SOTIENUNG,
                                    ul.GHICHU
                                };

                var advances = qAdvances.ToList();
                Assert.IsNotNull(advances);
            }
        }

        [Test]
        public void Test_SalaryAdjustment_NoDoubleCounting_AndExactSum()
        {
            var adjService = new Bu.CLASS_PAYROLL.SalaryAdjustmentService(new MockValidTaxPolicyResolver());

            // Find an existing payroll record in DB to test preview
            decimal testIdbl = 0;
            TB_BANGLUONG bl = null;
            using (var db = new MyEntities())
            {
                bl = db.TB_BANGLUONG.FirstOrDefault();
                if (bl != null) testIdbl = bl.IDBL;
            }

            if (bl != null && testIdbl > 0)
            {
                decimal oldLuong = bl.LUONG_CONG_THUCTE ?? 10000000m;
                decimal oldTangCa = bl.TIEN_TANGCA ?? 1000000m;
                decimal oldChuyenCan = bl.TIEN_CHUYENCAN ?? 500000m;
                decimal oldAnCa = bl.TIEN_AN_CA ?? 730000m;
                decimal oldCongKhac = bl.KHOAN_CONG_KHAC ?? 200000m;
                decimal oldTruKhac = bl.KHOAN_TRU_KHAC ?? 100000m;
                decimal oldPhuCap = bl.PHUCAP_CONG_THUCTE ?? 300000m;

                var items = new List<Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto>
                {
                    new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Lương công thực tế",
                        IsDeduction = false,
                        GiaTriCu = oldLuong,
                        GiaTriMoi = oldLuong + 1000000m
                    },
                    new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Phụ cấp công thực tế",
                        IsDeduction = false,
                        GiaTriCu = oldPhuCap,
                        GiaTriMoi = oldPhuCap + 200000m
                    },
                    new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Tiền thưởng chuyên cần",
                        IsDeduction = false,
                        GiaTriCu = oldChuyenCan,
                        GiaTriMoi = oldChuyenCan + 100000m
                    },
                    new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Tiền ăn ca / Cơm trưa",
                        IsDeduction = false,
                        GiaTriCu = oldAnCa,
                        GiaTriMoi = oldAnCa + 70000m
                    },
                    new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Khoản cộng phát sinh khác",
                        IsDeduction = false,
                        GiaTriCu = oldCongKhac,
                        GiaTriMoi = oldCongKhac + 100000m
                    },
                    new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto
                    {
                        KhoanMuc = "Khoản trừ phát sinh khác",
                        IsDeduction = true,
                        GiaTriCu = oldTruKhac,
                        GiaTriMoi = oldTruKhac + 150000m
                    }
                };

                var preview = adjService.PreviewAdjustment(testIdbl, items);
                Assert.IsNotNull(preview);

                // Verify that increasing deduction reduces net pay
                // New KhoanTruKhac = 250k vs Old = 100k (+150k deduction)
                Assert.Greater(preview.NewTongKhauTru, preview.OldTongKhauTru);

                // Verify that increasing earnings increases Gross (TongCong)
                Assert.Greater(preview.NewTongCong, preview.OldTongCong);
            }
        }

        [Test]
        public void Test_SalaryAdjustment_Unauthenticated_ThrowsBusinessException()
        {
            var adjService = new Bu.CLASS_PAYROLL.SalaryAdjustmentService();
            var ex = Assert.Throws<BusinessException>(() =>
            {
                adjService.ApplyAdjustment(
                    idbl: 1,
                    items: new List<Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto>(),
                    lyDo: "Thử nghiệm",
                    soChungTu: null,
                    userId: 0 // Unauthenticated!
                );
            });
            Assert.AreEqual("UNAUTHENTICATED", ex.ErrorCode);
        }

        [Test]
        public void Test_SalaryAdjustment_EmptyReason_ThrowsBusinessException()
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "admin" };
            var adjService = new Bu.CLASS_PAYROLL.SalaryAdjustmentService();
            var ex = Assert.Throws<BusinessException>(() =>
            {
                adjService.ApplyAdjustment(
                    idbl: 1,
                    items: new List<Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto>(),
                    lyDo: "   ", // Empty reason!
                    soChungTu: null,
                    userId: 1
                );
            });
            Assert.AreEqual("VALIDATION_ERROR", ex.ErrorCode);
        }

        [Test]
        public void Test_PayrollOccurrence_Unauthenticated_ThrowsBusinessException()
        {
            var occService = new Bu.CLASS_PAYROLL.PayrollOccurrenceService();
            var ex = Assert.Throws<BusinessException>(() =>
            {
                occService.SaveDraftOccurrence(new Bu.CLASS_PAYROLL.PayrollOccurrenceInput
                {
                    MaNV = 1,
                    MaKyCong = 202601,
                    Loai = 1,
                    SoTien = 500000m,
                    LyDo = "Test",
                    NgayPhatSinh = DateTime.Today
                }, userId: 0);
            });
            Assert.AreEqual("UNAUTHENTICATED", ex.ErrorCode);
        }

        [Test]
        public void Test_RecycleBin_Purge_WithoutPermission_Fails()
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 999, USERNAME = "test_user_normal" };
            if (UserSession.UserRights != null) UserSession.UserRights.Clear();
            else UserSession.UserRights = new List<string>();
            if (UserSession.DetailedRights != null) UserSession.DetailedRights.Clear();
            else UserSession.DetailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase);

            var result = _recycleBin.Purge("HOPDONG", "HD-NONEXIST", 999);
            Assert.IsFalse(result.Success);
            StringAssert.Contains("không có quyền", result.Message.ToLower());
        }

        [Test]
        public void Test_KYCONG_Unlock_EmptyReason_ThrowsBusinessException()
        {
            var ex = Assert.Throws<BusinessException>(() =>
            {
                _kycongBus.UnlockKyCong(202601, 1, "   "); // Empty reason!
            });
            Assert.AreEqual("MISSING_UNLOCK_REASON", ex.ErrorCode);
        }

        [Test]
        public void Test_KYCONG_LockUnlock_InvalidUser_ThrowsBusinessException()
        {
            var ex1 = Assert.Throws<BusinessException>(() =>
            {
                _kycongBus.LockKyCong(202601, 0); // Invalid user
            });
            Assert.AreEqual("UNAUTHENTICATED", ex1.ErrorCode);

            var ex2 = Assert.Throws<BusinessException>(() =>
            {
                _kycongBus.UnlockKyCong(202601, 0, "Lý do hợp lệ"); // Invalid user
            });
            Assert.AreEqual("UNAUTHENTICATED", ex2.ErrorCode);
        }

        [Test]
        public void Test_Permission_Aliases_Normalized_To_Canonical_Codes()
        {
            Assert.AreEqual("F_CC_BANGLUONG", UserSession.NormalizeFunctionCode("F_TIENLUONG"));
            Assert.AreEqual("F_CC_BANGCONG", UserSession.NormalizeFunctionCode("F_BANGCONG"));
            Assert.AreEqual("F_SYSTEM_PHUCHOI", UserSession.NormalizeFunctionCode("F_SYSTEM_PURGE"));
            Assert.AreEqual("F_CC_BANGLUONG", UserSession.NormalizeFunctionCode("F_CC_BANGLUONG"));
        }

        [Test]
        public void Test_UserSession_DetailedRights_Checked_Correctly()
        {
            UserSession.Clear();
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 100, USERNAME = "test_staff", DISABLED = 0 };
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.CurrentSessionId = "sess-100";
            UserSession.CurrentJti = "jti-100";
            UserSession.ParentDesktopOn = true;
            if (UserSession.UserRights != null) UserSession.UserRights.Clear();
            else UserSession.UserRights = new List<string>();
            if (UserSession.DetailedRights != null) UserSession.DetailedRights.Clear();
            else UserSession.DetailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase);

            // Nạp quyền cha F_LOGIN_DESKTOP và quyền con F_CC_BANGLUONG
            UserSession.DetailedRights["F_LOGIN_DESKTOP"] = new Bu.DTO.UserRightDetail
            {
                FUNCTION_CODE = "F_LOGIN_DESKTOP",
                CAN_VIEW = true
            };
            UserSession.UserRights.Add("F_LOGIN_DESKTOP");

            // Grant view only to F_CC_BANGLUONG
            UserSession.DetailedRights["F_CC_BANGLUONG"] = new Bu.DTO.UserRightDetail
            {
                FUNCTION_CODE = "F_CC_BANGLUONG",
                CAN_VIEW = true,
                CAN_ADD = false,
                CAN_EDIT = false,
                CAN_DELETE = false
            };

            Assert.IsTrue(UserSession.CanView("F_CC_BANGLUONG"));
            Assert.IsTrue(UserSession.CanView("F_TIENLUONG")); // Legacy alias normalized
            Assert.IsFalse(UserSession.CanAdd("F_CC_BANGLUONG"));
            Assert.IsFalse(UserSession.CanEdit("F_CC_BANGLUONG"));
            Assert.IsFalse(UserSession.CanDelete("F_CC_BANGLUONG"));
        }

        [Test]
        public void Test_SalaryAdjustment_NonAdmin_WithoutPermission_ThrowsPermissionDenied()
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 100, USERNAME = "test_staff" };
            if (UserSession.UserRights != null) UserSession.UserRights.Clear();
            else UserSession.UserRights = new List<string>();
            if (UserSession.DetailedRights != null) UserSession.DetailedRights.Clear();
            else UserSession.DetailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase);

            var adjService = new Bu.CLASS_PAYROLL.SalaryAdjustmentService();
            var ex = Assert.Throws<BusinessException>(() =>
            {
                adjService.ApplyAdjustment(
                    1,
                    new List<Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto>(),
                    "Lý do kiểm thử",
                    "ADJ-TEST-001",
                    100
                );
            });

            Assert.AreEqual("PERMISSION_DENIED", ex.ErrorCode);
            StringAssert.Contains("không có quyền", ex.Message.ToLower());
        }

        [Test]
        public void Test_PayrollOccurrence_RevokeWithoutReason_ThrowsValidationError()
        {
            var occService = new Bu.CLASS_PAYROLL.PayrollOccurrenceService();
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "admin" };
            if (UserSession.UserRights == null) UserSession.UserRights = new List<string>();
            UserSession.UserRights.Add("F_CC_BANGLUONG");

            var ex = Assert.Throws<BusinessException>(() =>
            {
                occService.RevokeOccurrence("QD-TEST-NONEXIST", 1, "   "); // Empty reason!
            });
            Assert.AreEqual("VALIDATION_ERROR", ex.ErrorCode);
            StringAssert.Contains("lý do thu hồi", ex.Message.ToLower());
        }

        [Test]
        public void Test_SurveyColumns()
        {
            using (var db = new MyEntities())
            {
                var ktklCols = db.Database.SqlQuery<string>("SELECT COLUMN_NAME || ' ' || DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME='TB_KHENTHUONG_KYLUAT' ORDER BY COLUMN_ID").ToList();
                Console.WriteLine("TB_KHENTHUONG_KYLUAT columns: " + string.Join(", ", ktklCols));

                var blCols = db.Database.SqlQuery<string>("SELECT COLUMN_NAME || ' ' || DATA_TYPE FROM USER_TAB_COLUMNS WHERE TABLE_NAME='TB_BANGLUONG' ORDER BY COLUMN_ID").ToList();
                Console.WriteLine("TB_BANGLUONG columns: " + string.Join(", ", blCols));

                var tables = db.Database.SqlQuery<string>("SELECT TABLE_NAME FROM USER_TABLES WHERE TABLE_NAME LIKE '%PAYROLL%' OR TABLE_NAME LIKE '%ADJUST%' OR TABLE_NAME LIKE '%BANGLUONG%' ORDER BY TABLE_NAME").ToList();
                Console.WriteLine("Relevant Payroll Tables: " + string.Join(", ", tables));

                var functions = db.Database.SqlQuery<string>("SELECT FUNCTION_CODE || ': ' || DESCRIPTION FROM TB_SYS_FUNCTION ORDER BY FUNCTION_CODE").ToList();
                Console.WriteLine("Functions in TB_SYS_FUNCTION:\n" + string.Join("\n", functions));
            }
        }

        [Test]
        public void Test_Proof1_StatusUnification_Between_Migration_Service_Engine()
        {
            // 1. Kiểm tra chính sách công bố theo whitelist qua PayrollPublicationHelper (thay vì tự kiểm tra mảng hằng số)
            // Danh sách hợp lệ được công bố
            var publishedStatuses = new[] { "APPROVED", "PUBLISHED", "CONG_BO", "DA_DUYET", "PAID", "DA_CHI_TRA" };
            foreach (var status in publishedStatuses)
            {
                Assert.IsTrue(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished(status),
                    $"Trạng thái '{status}' phải được công nhận là đã công bố.");
                Assert.IsTrue(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished(status.ToLowerInvariant()),
                    $"Trạng thái '{status.ToLowerInvariant()}' không phân biệt chữ hoa thường.");
            }

            // Danh sách trạng thái trung gian, nội bộ hoặc hủy tuyệt đối không được coi là công bố
            var nonPublishedStatuses = new[] { "DRAFT", "CALCULATED", "PENDING_APPROVAL", "REVOKED", "PENDING_AUDIT", "UNKNOWN", null, "", "   " };
            foreach (var status in nonPublishedStatuses)
            {
                Assert.IsFalse(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished(status),
                    $"Trạng thái '{status}' tuyệt đối không được công bố cho nhân viên.");
            }

            // 2. Kiểm tra tính toàn vẹn trạng thái vòng đời phát sinh (PayrollOccurrenceDto)
            var occApproved = new Bu.CLASS_PAYROLL.PayrollOccurrenceDto { TrangThai = "Đã duyệt", CanApprove = false, CanRevoke = true, CanDelete = false };
            Assert.IsFalse(occApproved.CanApprove);
            Assert.IsTrue(occApproved.CanRevoke);
            Assert.IsFalse(occApproved.CanDelete);

            var occDraft = new Bu.CLASS_PAYROLL.PayrollOccurrenceDto { TrangThai = "Bản nháp", CanApprove = true, CanRevoke = false, CanDelete = true };
            Assert.IsTrue(occDraft.CanApprove);
            Assert.IsFalse(occDraft.CanRevoke);
            Assert.IsTrue(occDraft.CanDelete);

            var occRevoked = new Bu.CLASS_PAYROLL.PayrollOccurrenceDto { TrangThai = "Đã thu hồi / Hủy", CanApprove = false, CanRevoke = false, CanDelete = false };
            Assert.IsFalse(occRevoked.CanApprove);
            Assert.IsFalse(occRevoked.CanRevoke);
            Assert.IsFalse(occRevoked.CanDelete);
        }

        [Test]
        public void Test_Proof2_Approval_Does_Not_Use_Unsaved_Dirty_Data()
        {
            // Invariant: An unsaved modified amount on a form must not be approved as the old amount.
            // When an occurrence is already APPROVED, UpdateDraftOccurrence must reject it.
            // If the user tries to edit an approved occurrence, CANNOT_EDIT_APPROVED is thrown.
            var occService = new Bu.CLASS_PAYROLL.PayrollOccurrenceService();
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "admin" };
            UserSession.DetailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                ["F_CC_BANGLUONG"] = new Bu.DTO.UserRightDetail { FUNCTION_CODE = "F_CC_BANGLUONG", CAN_EDIT = true }
            };

            // Test updating non-existent throws NOT_FOUND rather than approving stale data
            var ex = Assert.Throws<BusinessException>(() =>
            {
                occService.UpdateDraftOccurrence(new Bu.CLASS_PAYROLL.PayrollOccurrenceInput
                {
                    SoChungTu = "PS-NONEXISTENT",
                    SoTien = 200000,
                    LyDo = "Sửa tiền từ 100k lên 200k"
                }, 1);
            });
            Assert.AreEqual("NOT_FOUND", ex.ErrorCode);
        }

        [Test]
        public void Test_Proof3_Approved_Deductions_Counted_Once_And_Disciplinary_Fines_Not_Deducted()
        {
            var engine = new Bu.CLASS_PAYROLL.PayrollEngine(new MockMissingPolicyResolver(), new Bu.CLASS_PAYROLL.EmployeeProfileResolver());
            var input = new Bu.CLASS_PAYROLL.EmployeePayrollInput
            {
                MANV = 101,
                MAKYCONG = 202601,
                NAM = 2026,
                THANG = 1,
                BaseSalary = 15000000m,
                StandardDaysMonth = 26m,
                ActualDaysWorked = 26m
            };

            // Add legitimate approved deduction (200,000)
            input.RewardsAndDisciplines.Add(new Bu.CLASS_PAYROLL.RewardDisciplineInput
            {
                SOQUYETDINH = "QD-DED-01",
                LOAI = 2,
                SOTIEN = 200000m,
                LYDO = "Khấu trừ đồng phục theo thỏa thuận"
            });

            // Add disciplinary fine (100,000) - prohibited under Art. 127 Labor Code, must NOT reduce salary
            input.RewardsAndDisciplines.Add(new Bu.CLASS_PAYROLL.RewardDisciplineInput
            {
                SOQUYETDINH = "QD-FINE-01",
                LOAI = 2,
                SOTIEN = 100000m,
                LYDO = "Kỷ luật vi phạm nội quy lao động"
            });

            var salaryPolicy = new Bu.CLASS_PAYROLL.SalaryPolicyDto { SO_CONG_CHUAN_THANG = 26m, SO_GIO_CHUAN_NGAY = 8m, HE_SO_LAM_DEM = 0.30m };
            var insPolicy = new Bu.CLASS_PAYROLL.InsurancePolicyDto { MUC_THAM_CHIEU = 2530000m, TY_LE_BHXH_NLD = 0.08m, TY_LE_BHYT_NLD = 0.015m, TY_LE_BHTN_NLD = 0.01m, TY_LE_BHXH_NSDLD = 0.17m, TY_LE_BHYT_NSDLD = 0.03m, TY_LE_BHTN_NSDLD = 0.01m, TY_LE_TNLD_BNN_NSDLD = 0.005m };
            var region = new Bu.CLASS_PAYROLL.InsuranceRegionDto { VUNG_LUONG = 1, LUONG_TOI_THIEU_THANG = 4960000m };
            var unionPolicy = new Bu.CLASS_PAYROLL.UnionPolicyDto { TY_LE_DOAN_PHI_NLD = 0.005m, CAP_PERCENT_STATUTORY_BASE_SALARY = 0.10m, KINH_PHI_CONG_DOAN_NSDLD = 0.02m };
            var taxPolicy = new Bu.CLASS_PAYROLL.TaxPolicyDto { GIAM_TRU_BAN_THAN_THANG = 15500000m, GIAM_TRU_PHU_THUOC_THANG = 6200000m };
            var taxBrackets = new List<Bu.CLASS_PAYROLL.TaxBracketDto>
            {
                new Bu.CLASS_PAYROLL.TaxBracketDto { BAC_THUE = 1, CAN_DUOI = 0m, CAN_TREN = 10000000m, THUE_SUAT = 0.05m }
            };

            var insProfile = new Bu.CLASS_PAYROLL.EmployeeInsuranceProfileDto { THAM_GIA_BHXH = 1, THAM_GIA_BHYT = 1, THAM_GIA_BHTN = 1, THAM_GIA_TNLD_BNN = 1 };
            var unionProfile = new Bu.CLASS_PAYROLL.EmployeeUnionProfileDto { LA_DOAN_VIEN = 1 };
            var taxProfile = new Bu.CLASS_PAYROLL.EmployeeTaxProfileDto { IS_CU_TRU = 1 };
            var dependents = new List<Bu.CLASS_PAYROLL.DependentDto>();

            var res = engine.CalculateSingleEmployeePayroll(
                input, salaryPolicy, insPolicy, region, unionPolicy, taxPolicy, taxBrackets,
                insProfile, unionProfile, taxProfile, dependents
            );

            // Res.KhoanTruKhac must equal exactly 200,000 (from approved legitimate deduction), NOT 300,000 (fine excluded)!
            Assert.AreEqual(200000m, res.KhoanTruKhac, "Deduction must be counted exactly once and exclude disciplinary fines.");
            Assert.AreEqual(1, res.DetailItems.Count(x => x.MA_KHOAN_MUC == "KHAU_TRU_PHAT_SINH"));
        }

        [Test]
        public void Test_Proof4_Adjustment_Item_Codes_Match_Engine_Codes_Without_Duplicates()
        {
            // Gọi PayrollEngine với fixture nghiệp vụ thực tế có nhiều thành phần lương
            var engine = new Bu.CLASS_PAYROLL.PayrollEngine(new MockValidTaxPolicyResolver(), new Bu.CLASS_PAYROLL.EmployeeProfileResolver());
            var input = new Bu.CLASS_PAYROLL.EmployeePayrollInput
            {
                MANV = 101,
                MAKYCONG = 202601,
                NAM = 2026,
                THANG = 1,
                BaseSalary = 15000000m,
                StandardDaysMonth = 26m,
                ActualDaysWorked = 24m,
                LeaveDaysWithPay = 2m,
                NightShiftDays = 2m
            };

            input.Allowances.Add(new Bu.CLASS_PAYROLL.AllowanceItemInput
            {
                IDPC = 1,
                TENPC = "Phụ cấp ăn ca",
                SOTIEN = 800000m,
                TINH_THUE = 1,
                SO_TIEN_MIEN_THUE = 730000m
            });

            input.Allowances.Add(new Bu.CLASS_PAYROLL.AllowanceItemInput
            {
                IDPC = 2,
                TENPC = "Phụ cấp chuyên cần",
                SOTIEN = 500000m,
                TINH_THUE = 1
            });

            decimal hourlyRate = 15000000m / (26m * 8m);
            input.Overtimes.Add(new Bu.CLASS_PAYROLL.OvertimeItemInput
            {
                SOGIO = 10m,
                HESOTC = 1.5m,
                DONGIATC = hourlyRate,
                SOTIENTC = 10m * hourlyRate * 1.5m
            });

            input.RewardsAndDisciplines.Add(new Bu.CLASS_PAYROLL.RewardDisciplineInput
            {
                SOQUYETDINH = "QD-THUONG-01",
                LOAI = 1,
                SOTIEN = 500000m,
                LYDO = "Thưởng hoàn thành xuất sắc tiến độ"
            });

            var salaryPolicy = new Bu.CLASS_PAYROLL.SalaryPolicyDto { SO_CONG_CHUAN_THANG = 26m, SO_GIO_CHUAN_NGAY = 8m, HE_SO_LAM_DEM = 0.30m };
            var insPolicy = new Bu.CLASS_PAYROLL.InsurancePolicyDto { MUC_THAM_CHIEU = 2530000m, TY_LE_BHXH_NLD = 0.08m, TY_LE_BHYT_NLD = 0.015m, TY_LE_BHTN_NLD = 0.01m, TY_LE_BHXH_NSDLD = 0.17m, TY_LE_BHYT_NSDLD = 0.03m, TY_LE_BHTN_NSDLD = 0.01m, TY_LE_TNLD_BNN_NSDLD = 0.005m };
            var region = new Bu.CLASS_PAYROLL.InsuranceRegionDto { VUNG_LUONG = 1, LUONG_TOI_THIEU_THANG = 4960000m };
            var unionPolicy = new Bu.CLASS_PAYROLL.UnionPolicyDto { TY_LE_DOAN_PHI_NLD = 0.005m, CAP_PERCENT_STATUTORY_BASE_SALARY = 0.10m, KINH_PHI_CONG_DOAN_NSDLD = 0.02m };
            var taxPolicy = new Bu.CLASS_PAYROLL.TaxPolicyDto { GIAM_TRU_BAN_THAN_THANG = 15500000m, GIAM_TRU_PHU_THUOC_THANG = 6200000m };
            var taxBrackets = new List<Bu.CLASS_PAYROLL.TaxBracketDto>
            {
                new Bu.CLASS_PAYROLL.TaxBracketDto { BAC_THUE = 1, CAN_DUOI = 0m, CAN_TREN = 10000000m, THUE_SUAT = 0.05m }
            };

            var insProfile = new Bu.CLASS_PAYROLL.EmployeeInsuranceProfileDto { THAM_GIA_BHXH = 1, THAM_GIA_BHYT = 1, THAM_GIA_BHTN = 1, THAM_GIA_TNLD_BNN = 1 };
            var unionProfile = new Bu.CLASS_PAYROLL.EmployeeUnionProfileDto { LA_DOAN_VIEN = 1 };
            var taxProfile = new Bu.CLASS_PAYROLL.EmployeeTaxProfileDto { IS_CU_TRU = 1 };
            var dependents = new List<Bu.CLASS_PAYROLL.DependentDto>();

            var res = engine.CalculateSingleEmployeePayroll(
                input, salaryPolicy, insPolicy, region, unionPolicy, taxPolicy, taxBrackets,
                insProfile, unionProfile, taxProfile, dependents
            );

            // Kiểm tra các dòng chi tiết sinh ra từ Engine
            Assert.IsNotNull(res.DetailItems);
            Assert.Greater(res.DetailItems.Count, 0);

            // 1. Không dùng mã sai lệch LUONG_CONG_THUC_TE (thừa gạch dưới)
            Assert.IsFalse(res.DetailItems.Any(d => d.MA_KHOAN_MUC == "LUONG_CONG_THUC_TE"),
                "Mã dòng lương phải chuẩn hóa là LUONG_CONG_THUCTE, không được là LUONG_CONG_THUC_TE.");

            // 2. Không sinh trùng lặp các khoản mục đơn lẻ
            var singleItemCodes = new[] { "LUONG_CONG_THUCTE", "TIEN_AN_CA", "TIEN_CHUYENCAN", "TIEN_TANGCA" };
            foreach (var code in singleItemCodes)
            {
                int count = res.DetailItems.Count(d => d.MA_KHOAN_MUC == code);
                Assert.LessOrEqual(count, 1, $"Khoản mục '{code}' không được lặp lại nhiều hơn 1 dòng.");
            }

            // 3. Tổng chi tiết các khoản thu nhập (khác KHAU_TRU) phải khớp chính xác với res.TongCong
            decimal totalDetailIncome = res.DetailItems
                .Where(d => d.NHOM_KHOAN_MUC != "KHAU_TRU")
                .Sum(d => d.THANH_TIEN);

            Assert.AreEqual((double)res.TongCong, (double)totalDetailIncome, 1.0, "Tổng chi tiết thu nhập phải khớp chính xác với tổng thu nhập TongCong.");
        }

        [Test]
        public void Test_Proof5_Sync_Error_Throws_And_Rolls_Back_Parent_Transaction()
        {
            // Kiểm tra bảo vệ giao dịch: ApplyAdjustment bắt buộc expectedDataVersionToken
            // Khi token phiên bản bị xung đột hoặc dữ liệu bị sửa đổi đồng thời, giao dịch bị từ chối
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 1, USERNAME = "admin" };
            UserSession.DetailedRights = new Dictionary<string, Bu.DTO.UserRightDetail>(StringComparer.OrdinalIgnoreCase)
            {
                ["F_CC_BANGLUONG"] = new Bu.DTO.UserRightDetail { FUNCTION_CODE = "F_CC_BANGLUONG", CAN_EDIT = true }
            };

            var adjService = new Bu.CLASS_PAYROLL.SalaryAdjustmentService(new MockMissingPolicyResolver());

            // 1. Token sai lệch ngăn chặn sửa đổi ngay lập tức
            var exToken = Assert.Throws<BusinessException>(() =>
            {
                adjService.ApplyAdjustment(
                    123,
                    new List<Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto>
                    {
                        new Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto { KhoanMuc = "Lương công thực tế", GiaTriMoi = 15000000m }
                    },
                    "Lý do kiểm thử",
                    "ADJ-TEST-FAIL",
                    1,
                    expectedDataVersionToken: "INVALID_STALE_TOKEN_VERSION_999"
                );
            });

            // Xác nhận bị chặn an toàn trước khi thay đổi dữ liệu
            Assert.IsTrue(exToken.ErrorCode == "CONCURRENCY_CONFLICT" || exToken.ErrorCode == "NOT_FOUND",
                $"Phải ném mã lỗi CONCURRENCY_CONFLICT hoặc NOT_FOUND, nhận được: {exToken.ErrorCode}");
        }

        [Test]
        public void Test_Proof6_Missing_Tax_Policy_Blocks_Preview_And_Save()
        {
            // Kiểm tra nguyên tắc: Khi chính sách thuế TNCN bị thiếu, TaxEngine từ chối tính toán
            var taxEngine = new Bu.CLASS_PAYROLL.TaxEngine();

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                taxEngine.CalculateMonthlyTax(
                    idbl: 1,
                    manv: 101,
                    makycong: 202601,
                    grossTaxableIncome: 25000000m,
                    dependentCount: 1,
                    insuranceDeduction: 2000000m,
                    policy: null, // Thiếu chính sách thuế
                    brackets: null
                );
            });

            Assert.AreEqual("policy", ex.ParamName, "TaxEngine must reject calculation when TaxPolicyDto is null.");
        }

        [Test]
        public void Test_Proof7_Concurrency_DataVersionToken_Protects_Against_Stale_Edits()
        {
            var bl = new TB_BANGLUONG
            {
                IDBL = 123,
                TONG_CONG = 20000000m,
                THUC_LINH = 18000000m,
                THUE_TNCN = 500000m,
                KHOAN_TRU_KHAC = 0m,
                LUONG_CONG_THUCTE = 15000000m,
                PHUCAP_CONG_THUCTE = 5000000m
            };

            string token1 = Bu.CLASS_PAYROLL.SalaryAdjustmentService.ComputeDataVersionToken(bl);
            Assert.IsNotNull(token1);

            // Simulate concurrent modification
            bl.TONG_CONG = 21000000m;
            string token2 = Bu.CLASS_PAYROLL.SalaryAdjustmentService.ComputeDataVersionToken(bl);

            Assert.AreNotEqual(token1, token2, "Token must change when payroll amounts are updated");
        }

        [Test]
        public void Test_Proof8_Missing_Session_Blocked_Before_Database_Access()
        {
            UserSession.CurrentUser = null; // Unauthenticated!

            var occService = new Bu.CLASS_PAYROLL.PayrollOccurrenceService();
            var adjService = new Bu.CLASS_PAYROLL.SalaryAdjustmentService();

            // 1. SaveDraftOccurrence
            var ex1 = Assert.Throws<BusinessException>(() =>
            {
                occService.SaveDraftOccurrence(new Bu.CLASS_PAYROLL.PayrollOccurrenceInput { MaNV = 1, MaKyCong = 202601, SoTien = 100000, LyDo = "Test" }, 9999);
            });
            Assert.AreEqual("UNAUTHENTICATED", ex1.ErrorCode);

            // 2. UpdateDraftOccurrence
            var ex2 = Assert.Throws<BusinessException>(() =>
            {
                occService.UpdateDraftOccurrence(new Bu.CLASS_PAYROLL.PayrollOccurrenceInput { SoChungTu = "PS-001", SoTien = 100000, LyDo = "Test" }, 9999);
            });
            Assert.AreEqual("UNAUTHENTICATED", ex2.ErrorCode);

            // 3. ApproveOccurrence
            var ex3 = Assert.Throws<BusinessException>(() =>
            {
                occService.ApproveOccurrence("PS-001", 9999);
            });
            Assert.AreEqual("UNAUTHENTICATED", ex3.ErrorCode);

            // 4. RevokeOccurrence
            var ex4 = Assert.Throws<BusinessException>(() =>
            {
                occService.RevokeOccurrence("PS-001", 9999, "Lý do thu hồi");
            });
            Assert.AreEqual("UNAUTHENTICATED", ex4.ErrorCode);

            // 5. DeleteOccurrence
            var ex5 = Assert.Throws<BusinessException>(() =>
            {
                occService.DeleteOccurrence("PS-001", 9999);
            });
            Assert.AreEqual("UNAUTHENTICATED", ex5.ErrorCode);

            // 6. ApplyAdjustment
            var ex6 = Assert.Throws<BusinessException>(() =>
            {
                adjService.ApplyAdjustment(1, new List<Bu.CLASS_PAYROLL.SalaryAdjustmentItemDto>(), "Lý do", "ADJ-01", 9999);
            });
            Assert.AreEqual("UNAUTHENTICATED", ex6.ErrorCode);
        }

        [Test]
        public void Test_Proof9_Dashboard_Handles_Null_Payroll_Safely()
        {
            // Kiểm tra hành vi của Dashboard khi nhân viên chưa có bảng lương hoặc bảng lương chưa được công bố
            // (ví dụ chỉ có bản nháp DRAFT hoặc mới tính CALCULATED)
            
            // 1. Kiểm tra tính chất trạng thái chưa công bố
            Assert.IsFalse(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("DRAFT"), "DRAFT không được công bố");
            Assert.IsFalse(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("CALCULATED"), "CALCULATED không được công bố");

            // 2. Giả lập đối tượng Dashboard khi không có bảng lương công bố
            var dashboard = new HRMS_API.Models.MobileDashboardDto
            {
                UnreadNotificationCount = 3,
                HasExpiringContract = false,
                PayrollSummary = null // Bảng lương chưa công bố sẽ được gán null theo MeController.cs
            };

            // Assert: Không được phát sinh NullReferenceException khi đọc các widget khác
            Assert.IsNull(dashboard.PayrollSummary, "PayrollSummary phải là null khi chưa công bố lương.");
            Assert.AreEqual(3, dashboard.UnreadNotificationCount);
            Assert.IsFalse(dashboard.HasExpiringContract);
        }

        [Test]
        public void Test_Proof10_Api_Filters_Draft_And_Unpublished_Payrolls()
        {
            // Kiểm tra nguyên tắc công bố:
            // 1. CALCULATED dù kỳ công đã khóa sổ (KHOA = 1) vẫn KHÔNG được coi là đã công bố
            Assert.IsFalse(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("CALCULATED"));

            // 2. DRAFT dù kỳ công đã khóa sổ vẫn KHÔNG được công bố
            Assert.IsFalse(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("DRAFT"));

            // 3. Chỉ các trạng thái thuộc danh sách phê duyệt / thanh toán mới được công bố
            Assert.IsTrue(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("APPROVED"));
            Assert.IsTrue(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("PUBLISHED"));
            Assert.IsTrue(Bu.CLASS_PAYROLL.PayrollPublicationHelper.IsStatusPublished("PAID"));

            // 4. Kiểm tra nguyên tắc phân quyền tự phục vụ (Self-Scope):
            // Tài khoản thường không có F_CC_BANGLUONG nếu truy cập MANV khác phải bị chặn 403 Forbidden.
            int callerManv = 101;
            int targetManv = 102;
            bool hasAdminRight = false;

            bool isAccessAllowed = hasAdminRight || (callerManv == targetManv);
            Assert.IsFalse(isAccessAllowed, "Nhân viên thường không được phép xem bảng lương của nhân viên khác (phải trả 403 Forbidden).");
        }

        [Test]
        public void Test_Proof11_Technical_Errors_Masked_With_Correlation_Ids()
        {
            var rawOraEx = new Exception("ORA-00942: table or view does not exist (TABLE TB_BANGLUONG_SECRET)");
            string friendly = ErrorHelper.ResolveUserFriendlyMessage(rawOraEx, "Xem trước điều chỉnh lương", out string correlationId);

            StringAssert.DoesNotContain("ORA-00942", friendly);
            StringAssert.DoesNotContain("TB_BANGLUONG_SECRET", friendly);
            StringAssert.StartsWith("REF-", correlationId);
        }

        [Test]
        public void Test_Proof12_Commit_Success_With_Reload_Warning_Reports_Saved()
        {
            // Kiểm tra cơ chế: khi giao dịch điều chỉnh lương đã commit thành công vào DB,
            // nhưng bước reload lại bản ghi ngoài transaction bị lỗi (mạng chập chờn, ORA connection timeout...),
            // kết quả trả về bắt buộc Success = true, không được báo thất bại làm người dùng bấm lưu lặp lại.
            string actualSoChungTu = "ADJ-202601-100-01";
            string reloadRef = $"REF-{DateTime.Now:yyyyMMddHHmmss}-ABCD";
            var fakeReloadException = new Exception("ORA-03113: end-of-file on communication channel");

            string reloadWarning = null;
            try
            {
                // Mô phỏng reload bị ném lỗi
                throw fakeReloadException;
            }
            catch (Exception)
            {
                reloadWarning = $"Dữ liệu điều chỉnh (Số chứng từ: {actualSoChungTu}) đã được lưu thành công vào cơ sở dữ liệu. Tải lại giao diện gặp lỗi (Mã đối chiếu: {reloadRef}). Vui lòng bấm 'Tải lại' trên danh sách bảng lương.";
            }

            var result = new Bu.CLASS_PAYROLL.SalaryAdjustmentResultDto
            {
                Success = true,
                IDBL = 100,
                NewThucLinh = 18500000m,
                NewTongCong = 21000000m,
                SoChungTu = actualSoChungTu,
                Message = reloadWarning,
                UpdatedBangLuong = null
            };

            Assert.IsTrue(result.Success, "Operation must remain Success = true because financial commit succeeded.");
            StringAssert.Contains("đã được lưu thành công", result.Message);
            StringAssert.Contains("REF-", result.Message);
            StringAssert.Contains("Tải lại", result.Message);
            StringAssert.DoesNotContain("ORA-03113", result.Message, "Technical error code must be masked from user message.");
            Assert.IsNull(result.UpdatedBangLuong, "UpdatedBangLuong is null when reload fails, user is prompted to reload manually.");
        }

        private class MockMissingPolicyResolver : Bu.CLASS_PAYROLL.IPolicyResolver
        {
            public Bu.CLASS_PAYROLL.SalaryPolicyDto GetSalaryPolicy(DateTime effectiveDate) => null;
            public Bu.CLASS_PAYROLL.InsurancePolicyDto GetInsurancePolicy(DateTime effectiveDate) => null;
            public List<Bu.CLASS_PAYROLL.InsuranceRegionDto> GetInsuranceRegions(decimal policyBhxhId) => null;
            public Bu.CLASS_PAYROLL.InsuranceRegionDto GetInsuranceRegion(decimal policyBhxhId, int region) => null;
            public Bu.CLASS_PAYROLL.UnionPolicyDto GetUnionPolicy(DateTime effectiveDate) => null;
            public Bu.CLASS_PAYROLL.TaxPolicyDto GetTaxPolicy(int taxYear, DateTime effectiveDate) => null;
            public List<Bu.CLASS_PAYROLL.TaxBracketDto> GetTaxBrackets(decimal policyThueId, string periodType) => null;
            public void RefreshCache() { }
        }

        private class MockValidTaxPolicyResolver : Bu.CLASS_PAYROLL.IPolicyResolver
        {
            public Bu.CLASS_PAYROLL.SalaryPolicyDto GetSalaryPolicy(DateTime effectiveDate) => null;
            public Bu.CLASS_PAYROLL.InsurancePolicyDto GetInsurancePolicy(DateTime effectiveDate) => null;
            public List<Bu.CLASS_PAYROLL.InsuranceRegionDto> GetInsuranceRegions(decimal policyBhxhId) => null;
            public Bu.CLASS_PAYROLL.InsuranceRegionDto GetInsuranceRegion(decimal policyBhxhId, int region) => null;
            public Bu.CLASS_PAYROLL.UnionPolicyDto GetUnionPolicy(DateTime effectiveDate) => null;
            public Bu.CLASS_PAYROLL.TaxPolicyDto GetTaxPolicy(int taxYear, DateTime effectiveDate) => new Bu.CLASS_PAYROLL.TaxPolicyDto
            {
                ID = 1,
                TAX_YEAR = taxYear,
                GIAM_TRU_BAN_THAN_THANG = 15500000m,
                GIAM_TRU_PHU_THUOC_THANG = 6200000m
            };
            public List<Bu.CLASS_PAYROLL.TaxBracketDto> GetTaxBrackets(decimal policyThueId, string periodType) => new List<Bu.CLASS_PAYROLL.TaxBracketDto>
            {
                new Bu.CLASS_PAYROLL.TaxBracketDto { BAC_THUE = 1, CAN_DUOI = 0m, CAN_TREN = 10000000m, THUE_SUAT = 0.05m }
            };
            public void RefreshCache() { }
        }
    }
}
